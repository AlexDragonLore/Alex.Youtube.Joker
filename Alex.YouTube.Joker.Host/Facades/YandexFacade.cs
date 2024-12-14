using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alex.YouTube.Joker.Host.Facades
{
    public class YandexFacade : IYandexFacade
    {
        private readonly HttpClient _httpClient;
        private readonly string _iamToken;

        // v3-голоса для русского языка (уточнить по документации)
        private static readonly string[] Voices = { "oksana_v3", "jane_v3", "alyss_v3", "omazh_v3", "zahar_v3", "ermil_v3" };
        private string RandomVoice => Voices[Random.Shared.Next(Voices.Length)];

        // Настройки TTS
        private const string DefaultAudioFormat = "AUDIO_FORMAT_MP3"; // v3 использует перечисления в виде строк
        private const double DefaultSpeakingRate = 1.0;

        public YandexFacade(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> ToVoice(string text, CancellationToken token)
        {
            var requestObject = new
            {
                text = text,
                audioFormat = DefaultAudioFormat,
                model = new 
                {
                    voice = RandomVoice,
                },
                // speakingRate соответствует скорости: 1.0 — обычная скорость
                speakingRate = DefaultSpeakingRate
            };

            var json = JsonSerializer.Serialize(requestObject);
            using var requestContent = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _httpClient.PostAsync("https://tts.api.cloud.yandex.net/speech/v3/tts:utteranceSynthesis", requestContent, token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(token);
                throw new Exception($"Ошибка при обращении к Yandex TTS v3 API: {response.StatusCode} - {errorContent}");
            }

            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp3");

            await using var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await using var responseStream = await response.Content.ReadAsStreamAsync(token);
            using var reader = new StreamReader(responseStream, Encoding.UTF8);

            // Потоковый ответ: каждая строка — отдельный JSON-объект UtteranceSynthesisResponse
            // Пример: {"audioChunk":"...base64...","utteranceStartOffsetMs":"0","charactersProcessed":"..."}
            // Нам нужен audioChunk - base64 строка с частью аудио.
            
            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var responseMessage = JsonSerializer.Deserialize<UtteranceSynthesisResponse>(line);
                if (responseMessage?.AudioChunk != null)
                {
                    var audioBytes = Convert.FromBase64String(responseMessage.AudioChunk);
                    await fileStream.WriteAsync(audioBytes, 0, audioBytes.Length, token);
                }
            }

            return tempFilePath;
        }

        private class UtteranceSynthesisResponse
        {
            [JsonPropertyName("audioChunk")]
            public string? AudioChunk { get; set; }

            [JsonPropertyName("utteranceStartOffsetMs")]
            public string? UtteranceStartOffsetMs { get; set; }

            [JsonPropertyName("charactersProcessed")]
            public string? CharactersProcessed { get; set; }
        }
    }
}
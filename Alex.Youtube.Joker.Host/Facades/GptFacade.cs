using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer; // Установите пакет Newtonsoft.Json через NuGet

namespace Alex.Youtube.Joker.Facades
{
    public class GptFacade
    {
        private readonly HttpClient _httpClient;
        private readonly string _openAiApiKey;
        private readonly string _textToSpeechApiKey;

        public GptFacade(HttpClient httpClient, string openAiApiKey, string textToSpeechApiKey)
        {
            _httpClient = httpClient;
            _openAiApiKey = openAiApiKey;
            _textToSpeechApiKey = textToSpeechApiKey;
        }

        public async Task<string> CreateJoke(string theme, CancellationToken token)
        {
            var requestBody = new
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new { role = "user", content = $"Расскажи смешную шутку на тему: {theme}" }
                },
                max_tokens = 60,
                temperature = 0.7,
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);

            using var response =
                await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", requestContent, token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(token);
                throw new Exception($"Ошибка при обращении к OpenAI API: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(token);
            dynamic responseObject = JsonSerializer.Deserialize(responseContent);

            string joke = responseObject.choices[0].message.content.ToString().Trim();

            return joke;
        }

        public async Task<FileInfo> ToVoice(string text, CancellationToken token)
        {
            // Используем Yandex SpeechKit API для синтеза речи
            var requestBody = new
            {
                text = text,
                voice = "alena", // Выберите голос
                format = "lpcm",
                sampleRateHertz = 48000
            };

            var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Api-Key", _textToSpeechApiKey);

            var response = await _httpClient.PostAsync("https://tts.api.cloud.yandex.net/speech/v1/tts:synthesize",
                requestContent, token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Ошибка при обращении к сервису синтеза речи: {response.StatusCode} - {errorContent}");
            }

            var audioBytes = await response.Content.ReadAsByteArrayAsync();

            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");
            await File.WriteAllBytesAsync(tempFilePath, audioBytes, token);

            return new FileInfo(tempFilePath);
        }
    }
}
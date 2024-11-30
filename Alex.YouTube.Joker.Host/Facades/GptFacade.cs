using System.Net.Http.Headers;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Alex.YouTube.Joker.Host.Facades
{
    public class GptFacade : IGptFacade
    {
        private readonly HttpClient _httpClient;

        public GptFacade(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> CreateJoke(string theme, CancellationToken token)
        {
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "user", content = $"Расскажи смешную шутку на тему: {theme}" }
                },
                max_tokens = 60,
                temperature = 0.7,
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8,
                "application/json");

            using var response =
                await _httpClient.PostAsync("/v1/chat/completions", requestContent, token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(token);
                throw new Exception($"Ошибка при обращении к OpenAI API: {response.StatusCode} - {errorContent}");
            }

            var responseContent = await response.Content.ReadAsStringAsync(token);
            var responseObject = JsonSerializer.Deserialize<dynamic>(responseContent!);

            string joke = responseObject.choices[0].message.content.ToString().Trim();

            return joke;
        }

        public async Task<FileInfo> ToVoice(string text, CancellationToken token)
        {
            var requestBody = new
            {
                model = "tts-1",
                input = text,
                voice = "alloy"
            };

            var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8,
                "application/json");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _openAiApiKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/audio/speech")
            {
                Content = requestContent
            };

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(token);
                throw new Exception($"Ошибка при обращении к OpenAI TTS API: {response.StatusCode} - {errorContent}");
            }

            var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mp3");

            await using var fileStream =
                new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

            await response.Content.CopyToAsync(fileStream, token);


            return new FileInfo(tempFilePath);
        }
    }
}
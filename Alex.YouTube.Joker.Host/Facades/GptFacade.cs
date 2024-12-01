using System.Net.Http.Headers;
using System.Text;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.Host.Facades.Models;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Alex.YouTube.Joker.Host.Facades;

public class GptFacade : IGptFacade
{
    private static readonly List<string> Voices = ["alloy", "echo", "fable", "onyx", "nova", "shimmer"];
    private readonly HttpClient _httpClient;

    public GptFacade(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateText(string prompt, CancellationToken token)
    {
        var requestBody = new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            max_tokens = 60,
            temperature = 0.7,
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8,
            "application/json");

        using var response =
            await _httpClient.PostAsync("v1/chat/completions", requestContent, token);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(token);
            throw new Exception($"Ошибка при обращении к OpenAI API: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(token);
        var gptResponse = JsonSerializer.Deserialize<GptResponse>(responseContent!);

        var joke = gptResponse!.Choices[0].Message.Content;

        return joke;
    }

    public async Task<string> ToVoice(string text, CancellationToken token)
    {
        var requestBody = new
        {
            model = "tts-1",
            input = text,
            voice = Voices[Random.Shared.Next(0, Voices.Count - 1)],
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8,
            "application/json");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.PostAsync("v1/audio/speech", requestContent, token);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(token);
            throw new Exception($"Ошибка при обращении к OpenAI TTS API: {response.StatusCode} - {errorContent}");
        }

        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.mpeg");

        await using var fileStream =
            new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);

        await response.Content.CopyToAsync(fileStream, token);

        return tempFilePath;
    }
    
    public async Task<string> ToImage(string prompt, CancellationToken token)
    {
        var requestBody = new
        {
            prompt = prompt,
            n = 1, // Количество изображений
            size = "1024x1024", // Поддерживаемый размер изображения
            response_format = "b64_json" // Формат ответа - base64
        };

        var requestContent = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        using var response = await _httpClient.PostAsync("v1/images/generations", requestContent, token);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(token);
            throw new Exception($"Ошибка при обращении к OpenAI DALL·E API: {response.StatusCode} - {errorContent}");
        }

        var responseContent = await response.Content.ReadAsStringAsync(token);
        var imageResponse = JsonSerializer.Deserialize<ImageResponse>(responseContent!);

        if (imageResponse == null || imageResponse.Data == null || imageResponse.Data.Count == 0)
        {
            throw new Exception("Не удалось получить изображение от OpenAI API.");
        }

        var base64Image = imageResponse.Data[0].B64Json;

        // Декодируем Base64 в файл
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.png");
        var imageBytes = Convert.FromBase64String(base64Image);

        await File.WriteAllBytesAsync(tempFilePath, imageBytes, token);

        return tempFilePath;
    }
}
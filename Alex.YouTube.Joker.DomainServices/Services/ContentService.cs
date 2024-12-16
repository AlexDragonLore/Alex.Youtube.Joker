using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Microsoft.Extensions.Logging;

namespace Alex.YouTube.Joker.DomainServices.Services;

public class ContentService : IContentService
{
    private readonly IGptFacade _gptFacade;
    private readonly IImageService _imageService;
    private readonly ILogger<ContentService> _logger;
    private readonly SemaphoreSlim _semaphore = new(3);
    
    
    public ContentService(IGptFacade gptFacade, IImageService imageService, ILogger<ContentService> logger)
    {
        _gptFacade = gptFacade;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Joke>> GetJokesForShort(string? theme, CancellationToken ct)
    {
        theme ??= await _gptFacade.GenerateText(
            "Придумай смешную тему для шуток, тема должна быть максимально случайна и актуальна. Напиши ТОЛЬКО тему, без пояснений и предисловий.",
            false, ct);
        
        var jokes = await Task.WhenAll(Enumerable.Range(0, 6).Select(s => GetJoke(theme, ct)));

        return jokes;
    }

    public async Task<IReadOnlyCollection<ZodiacPredict>> GetZodiacs(DateOnly date, CancellationToken ct)
    {
        var jokes = await Task.WhenAll(Themes.Zodiacs.Select(s => GetZodiac(s, date, ct)));

        return jokes;
    }

    public async Task<ZodiacPredict> GetZodiac(string name, DateOnly date, CancellationToken ct)
    {
        var predictText = await _gptFacade.GenerateText(
            $"Воззови к древней мудрости звезд и создай краткое, но глубокое предсказание для знака зодиака {name} на {date}. Да не будет там ни слова лишнего.", false, ct);
        
        predictText = predictText.Replace(Environment.NewLine, "");
        
        _logger.LogInformation("Generated predict {joke}", predictText);
        string voice = null;
        for (int i = 0; i < 5; i++)
        {
            try
            {
                voice = await _gptFacade.ToVoice(predictText, ct);
            }
            catch
            {
                if (i == 4)
                {
                    throw;
                }
            }
        }
        
        var image = _imageService.GetRandomImageWithText(predictText);
        
        return new ZodiacPredict
        {
            Name = name,
            Text = predictText,
            ImagePath = image,
            AudioPath = voice!
        };
    }

    private async Task<Joke> GetJoke(string theme, CancellationToken ct)
    {
        await _semaphore.WaitAsync(ct);
        try
        {
            var joke = await _gptFacade.GenerateText(
                $"Придумай одну действительно смешную, понятную и жизненную шутку на тему: {theme}. Пусть шутка будет короткой и основанной на узнаваемой ситуации, которую легко представить. Напиши ТОЛЬКО шутку, без пояснений, и предисловий, и не используй спец символов, и перенос строки.",
                true, ct);

            joke = joke.Replace(Environment.NewLine, "");
            _logger.LogInformation("Generated Joke {joke}", joke);
            var voice = await _gptFacade.ToVoice(joke, ct);
            var image = _imageService.GetRandomImageWithText(joke);

            return new Joke
            {
                Theme = theme,
                Text = joke,
                ImagePath = image,
                AudioPath = voice
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Microsoft.Extensions.Logging;

namespace Alex.YouTube.Joker.DomainServices.Services;

public class JokeService : IJokeService
{
    private readonly IGptFacade _gptFacade;
    private readonly IImageService _imageService;
    private readonly ILogger<JokeService> _logger;

    public JokeService(IGptFacade gptFacade, IImageService imageService, ILogger<JokeService> logger)
    {
        _gptFacade = gptFacade;
        _imageService = imageService;
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<Joke>> GetJokesForShort(string theme, CancellationToken ct)
    {
        var image = _imageService.GetRandomImageWithText("ds");
        var jokes = await Task.WhenAll(Enumerable.Range(0, 6).Select(s => GetJoke(theme, ct)));

        return jokes;
    }

    private async Task<Joke> GetJoke(string theme, CancellationToken ct)
    {
        var joke = await _gptFacade.GenerateText($"Расскажи смешную шутку на тему: {theme} шутка должна быть жизненой. Напиши ТОЛЬКО шутку.", ct);
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
}
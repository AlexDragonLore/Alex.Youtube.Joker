using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;

namespace Alex.YouTube.Joker.DomainServices.Services;

public class JokeService : IJokeService
{
    private readonly IGptFacade _gptFacade;

    public JokeService(IGptFacade gptFacade)
    {
        _gptFacade = gptFacade;
    }

    public async Task<IReadOnlyCollection<Joke>> GetJokesForShort(string theme, CancellationToken ct)
    {
        var jokes = await Task.WhenAll(Enumerable.Range(0, 2).Select(s=> GetJoke(theme, ct)));
        
        return jokes; 
    }

    private async Task<Joke> GetJoke(string theme, CancellationToken ct)
    {
        var joke = await _gptFacade.GenerateText($"Расскажи смешную шутку на тему: {theme}. Напиши ТОЛЬКО шутку.", ct);
        var voice = _gptFacade.ToVoice(joke, ct);
        var image = _gptFacade.ToImage(joke, ct);

        return new Joke
        {
            Theme = theme,
            Text = joke,
            ImagePath = await image,
            AudioPath = await voice
        };
    }
}
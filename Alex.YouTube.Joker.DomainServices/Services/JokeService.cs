using System.Text;
using Alex.YouTube.Joker.DomainServices.Facades;

namespace Alex.YouTube.Joker.DomainServices.Services;

public class JokeService : IJokeService
{
    private readonly IGptFacade _gptFacade;

    public JokeService(IGptFacade gptFacade)
    {
        _gptFacade = gptFacade;
    }

    public async Task<string> GetJokesForShort(string theme, CancellationToken ct)
    {
        var builder = new StringBuilder();
        while (builder.Length < 500)
        {
            var joke = await _gptFacade.CreateJoke(theme, ct);
            builder.Append(joke);
            builder.Append(".....");
        }
        var voice = await _gptFacade.ToVoice(builder.ToString(), ct);
        return voice; 
    }
}
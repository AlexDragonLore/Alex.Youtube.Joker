namespace Alex.YouTube.Joker.DomainServices.Facades;

public interface IGptFacade
{
    Task<string> CreateJoke(string theme, CancellationToken token);
    Task<string> ToVoice(string text, CancellationToken token);
}
namespace Alex.YouTube.Joker.DomainServices.Facades;

public interface IGptFacade
{
    Task<string> GenerateText(string prompt, bool smart, CancellationToken token);
    Task<string> ToVoice(string text, CancellationToken token);
    Task<string> ToImage(string prompt, CancellationToken token);
}
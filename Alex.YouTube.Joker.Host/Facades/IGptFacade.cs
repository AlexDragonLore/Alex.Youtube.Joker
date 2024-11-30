namespace Alex.YouTube.Joker.Host.Facades;

public interface IGptFacade
{
    Task<string> CreateJoke(string theme, CancellationToken token);
    Task<FileInfo> ToVoice(string text, CancellationToken token);
}
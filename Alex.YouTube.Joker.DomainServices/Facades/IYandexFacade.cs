namespace Alex.YouTube.Joker.Host.Facades;

public interface IYandexFacade
{
    Task<string> ToVoice(string text, CancellationToken token);
}
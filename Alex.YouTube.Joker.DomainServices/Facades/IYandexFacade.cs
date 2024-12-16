namespace Alex.YouTube.Joker.DomainServices.Facades;

public interface IYandexFacade
{
    Task<string> ToVoice(string text, CancellationToken token);
}
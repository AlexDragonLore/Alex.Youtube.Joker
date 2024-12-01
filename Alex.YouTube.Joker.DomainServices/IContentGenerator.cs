namespace Alex.YouTube.Joker.DomainServices;

public interface IContentGenerator
{
    Task<string> GetShort(string theme, CancellationToken token);
}
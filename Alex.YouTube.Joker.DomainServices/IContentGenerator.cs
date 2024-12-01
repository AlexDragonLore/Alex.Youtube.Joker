namespace Alex.YouTube.Joker.DomainServices;

public interface IContentGenerator
{
    Task GenerateShorts(string theme, CancellationToken token);
}
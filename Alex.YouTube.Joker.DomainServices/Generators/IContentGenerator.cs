namespace Alex.YouTube.Joker.DomainServices.Generators;

public interface IContentGenerator
{
    Task GenerateShorts(string theme, CancellationToken token);
}
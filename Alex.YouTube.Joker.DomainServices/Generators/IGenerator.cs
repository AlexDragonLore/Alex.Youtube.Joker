namespace Alex.YouTube.Joker.DomainServices.Generators;

public interface IGenerator
{
    Task GenerateShorts(CancellationToken token);
}
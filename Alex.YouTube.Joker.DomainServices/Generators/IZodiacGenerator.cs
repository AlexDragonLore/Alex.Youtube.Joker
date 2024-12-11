namespace Alex.YouTube.Joker.DomainServices.Generators;

public interface IZodiacGenerator
{
    Task GenerateShorts(CancellationToken token);
}
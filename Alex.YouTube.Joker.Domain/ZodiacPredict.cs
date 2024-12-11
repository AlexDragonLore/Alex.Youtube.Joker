namespace Alex.YouTube.Joker.Domain;

public class ZodiacPredict
{
    public required string Name { get; init; }
    public required string Text { get; init; }
    public required string ImagePath { get; init; }
    public required string AudioPath { get; init; }
}
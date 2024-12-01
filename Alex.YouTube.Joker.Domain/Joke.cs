namespace Alex.YouTube.Joker.Domain;

public class Joke
{
    public required string Theme { get; init; }
    public required string Text { get; init; }
    public required string ImagePath { get; init; }
    public required string AudioPath { get; init; }
}
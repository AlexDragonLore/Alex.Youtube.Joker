namespace Alex.YouTube.Joker.Domain;

public class VideoRequest
{
    public required string ImagePath { get; init; }
    public required string AudioPath { get; init; }
    public required string OutputVideoPath { get; init; }
    public required string JokeText { get; init; }
    public required string ThemeText { get; init; }
}
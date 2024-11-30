namespace Alex.YouTube.Joker.Domain;

public class YouTubeShort
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string FilePath { get; init; }
    public required IList<string> Tags { get; init; }
}
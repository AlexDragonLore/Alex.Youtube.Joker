namespace Alex.YouTube.Joker.Domain;

public class YouTube
{
    public required IReadOnlyCollection<Channel> Channels { get; init; }
}
namespace Alex.YouTube.Joker.Domain;

public class Channel
{
    public required string Name { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string RefreshToken { get; init; }
    public required string AccessToken { get; set; }
}
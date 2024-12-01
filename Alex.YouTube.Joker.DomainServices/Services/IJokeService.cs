namespace Alex.YouTube.Joker.DomainServices.Services;

public interface IJokeService
{
    Task<string> GetJokesForShort(string theme, CancellationToken ct);
}
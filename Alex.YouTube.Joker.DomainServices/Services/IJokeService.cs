using Alex.YouTube.Joker.Domain;

namespace Alex.YouTube.Joker.DomainServices.Services;

public interface IJokeService
{
    Task<IReadOnlyCollection<Joke>> GetJokesForShort(string theme, CancellationToken ct);
}
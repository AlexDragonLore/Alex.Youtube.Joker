using Alex.YouTube.Joker.Domain;

namespace Alex.YouTube.Joker.DomainServices.Services;

public interface IContentService
{
    Task<IReadOnlyCollection<Joke>> GetJokesForShort(string? theme, CancellationToken ct);
    Task<IReadOnlyCollection<ZodiacPredict>> GetZodiacs(DateOnly date, CancellationToken ct);
}
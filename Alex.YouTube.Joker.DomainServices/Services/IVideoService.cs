using Alex.YouTube.Joker.Domain;

namespace Alex.YouTube.Joker.DomainServices.Services;

public interface IVideoService
{
    Task CreateVideoWithXabe(VideoRequest request, CancellationToken token);
}
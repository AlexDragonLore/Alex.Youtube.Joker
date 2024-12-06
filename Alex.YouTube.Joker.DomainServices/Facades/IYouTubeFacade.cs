using Alex.YouTube.Joker.Domain;

namespace Alex.YouTube.Joker.DomainServices.Facades;

public interface IYouTubeFacade
{
    Task UploadShort(YouTubeShort shorts, CancellationToken token);
    Task List(CancellationToken token);
}
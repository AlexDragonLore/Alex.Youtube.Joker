using Alex.YouTube.Joker.Domain;

namespace Alex.YouTube.Joker.DomainServices.Facades;

public interface IYouTubeFacade
{
    Task UploadShort(YouTubeShort shorts, Channel channel, CancellationToken token);
    Task JustAuth2(Channel channel);
}
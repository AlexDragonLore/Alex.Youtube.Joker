using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Channel = Alex.YouTube.Joker.Domain.Channel;

namespace Alex.YouTube.Joker.Host.Facades;

public class YouTubeFacade : IYouTubeFacade
{
    private async Task<YouTubeService> UseAccessToken(Channel channel)
    {
        var credential = GoogleCredential.FromAccessToken(await RefreshAccessToken(channel));

        return new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MyContainerApp"
        });
    }

    private async Task<string> RefreshAccessToken(Channel channel)
    {

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = channel.ClientId,
                ClientSecret = channel.ClientSecret
            }
        });

        var newToken = await flow.RefreshTokenAsync("user", channel.RefreshToken, CancellationToken.None);
        return newToken.AccessToken;
    }


    public async Task UploadShort(YouTubeShort shorts, Channel channel, CancellationToken token)
    {
        // Authenticate and get the YouTube service
        using var youtubeService = await UseAccessToken(channel);

        // Create a new video resource
        var video = new Video
        {
            Snippet = new VideoSnippet
            {
                Title = shorts.Title,
                Description = shorts.Description,
                Tags = shorts.Tags,
                CategoryId = "22", // "People & Blogs" category
            },
            Status = new VideoStatus
            {
                PrivacyStatus = "public" // or "unlisted", "private"
            }
        };

        // The path to the video file
        var filePath = shorts.FilePath;

        await using var fileStream = new FileStream(filePath, FileMode.Open);
        // Create the request for uploading the video
        var videosInsertRequest =
            youtubeService.Videos.Insert(video, "snippet,status", fileStream, GetMimeType(filePath));

        // Event handler for upload progress
        videosInsertRequest.ProgressChanged += VideosInsertRequest_ProgressChanged;
        // Event handler for upload response received
        videosInsertRequest.ResponseReceived += VideosInsertRequest_ResponseReceived;

        // Upload the video
        var result = await videosInsertRequest.UploadAsync(token);
        if (result.Status == UploadStatus.Failed)
        {
            throw result.Exception;
        }
    }

    private void VideosInsertRequest_ProgressChanged(IUploadProgress progress)
    {
        switch (progress.Status)
        {
            case UploadStatus.Uploading:
                Console.WriteLine($"{progress.BytesSent} bytes sent.");
                break;

            case UploadStatus.Completed:
                Console.WriteLine("Upload completed.");
                break;

            case UploadStatus.Failed:
                Console.WriteLine($"An error prevented the upload from completing.\n{progress.Exception}");
                break;
        }
    }

    private void VideosInsertRequest_ResponseReceived(Video video)
    {
        Console.WriteLine($"Video id '{video.Id}' was successfully uploaded as a YouTube Short.");
    }

    public async Task JustAuth2(Channel channel)
    {
        var s= await Auth2(channel);
        var r =s.Channels.List("snippet");
        await r.ExecuteAsync();
    }
    
    public async Task<YouTubeService> Auth2(Channel channel)
    {
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = channel.ClientId,
                ClientSecret = channel.ClientSecret,
            },
            new[] { YouTubeService.Scope.YoutubeReadonly, YouTubeService.Scope.YoutubeUpload },
            "user",
            CancellationToken.None,
            new FileDataStore("tokens", true) // Локальная директория для токенов
        );

        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = GetType().ToString()
        });

        return youtubeService;
    }

    private string GetMimeType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        switch (extension)
        {
            case ".mp4": return "video/mp4";
            case ".mov": return "video/quicktime";
            case ".avi": return "video/x-msvideo";
            // Add more cases as needed
            default: return "video/*";
        }
    }
}
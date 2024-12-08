using System.Text.Json;
using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Requests;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Alex.YouTube.Joker.Host.Facades;

public class YouTubeFacade : IYouTubeFacade
{
    private readonly string _credentialsFilePath;
    private readonly string _сlientSecret;
    private readonly string _сlientId;
    private readonly string _refreshToken;

    public YouTubeFacade(IConfiguration configuration)
    {
        _сlientSecret = configuration["YouTube:ClientSecret"]!;
        _сlientId = configuration["YouTube:ClientId"]!;
        _refreshToken = configuration["YouTube:RefreshToken"]!;
        _credentialsFilePath = Path.Combine(AppContext.BaseDirectory, "joker-443412-fb9cf01a4005.json");
    }

    public async Task List(CancellationToken token)
    {
        // Authenticate and get the YouTube service
        var youtubeService = await Auth2();


        // Create a new video resource
        var channelsListRequest = youtubeService.Channels.List("snippet");
        channelsListRequest.Mine = true;
        var channelsListResponse = await channelsListRequest.ExecuteAsync();
        var d = JsonSerializer.Serialize(channelsListResponse);
    }

    public async Task<YouTubeService> UseAccessToken()
    {
        var credential = GoogleCredential.FromAccessToken(await RefreshAccessToken());

        return new YouTubeService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "MyContainerApp"
        });
    }

    public async Task<string> RefreshAccessToken()
    {

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _сlientId,
                ClientSecret = _сlientSecret
            }
        });

        var newToken = await flow.RefreshTokenAsync("user", _refreshToken, CancellationToken.None);
        return newToken.AccessToken;
    }


    public async Task UploadShort(YouTubeShort shorts, CancellationToken token)
    {
        // Authenticate and get the YouTube service
        var youtubeService = await UseAccessToken();

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
        await videosInsertRequest.UploadAsync(token);
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

    private async Task<YouTubeService> Auth()
    {

        var credential = GoogleCredential.FromFile(_credentialsFilePath).CreateScoped(new[]
        {
            YouTubeService.Scope.YoutubeReadonly, // Для чтения данных о канале
            YouTubeService.Scope.YoutubeUpload,
            YouTubeService.Scope.YoutubeChannelMembershipsCreator
            // Для загрузки видео
        });

        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "AlexYouTubeJoker"
        });

        return youtubeService;
    }

    private async Task<YouTubeService> Auth2()
    {
        var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
            new ClientSecrets
            {
                ClientId = _сlientId,
                ClientSecret = _сlientSecret,
            },
            new[] { YouTubeService.Scope.YoutubeReadonly, YouTubeService.Scope.YoutubeUpload },
            "user",
            CancellationToken.None,
            new FileDataStore("tokens", true) // Локальная директория для токенов
        );

        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = this.GetType().ToString()
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
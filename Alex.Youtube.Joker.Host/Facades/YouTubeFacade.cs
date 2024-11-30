using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Alex.YouTube.Joker.Domain;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace Alex.Youtube.Joker.Facades
{
    public class YouTubeFacade
    {
        public YouTubeFacade()
        {
        }

        public async Task UploadShort(YouTubeShort shorts, CancellationToken token)
        {
            // Authenticate and get the YouTube service
            var youtubeService = await Auth();

            // Create a new video resource
            var video = new Video
            {
                Snippet = new VideoSnippet
                {
                    Title = shorts.Title,
                    Description = shorts.Description,
                    Tags = shorts.Tags,
                    CategoryId = "22" // "People & Blogs" category
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
            var videosInsertRequest = youtubeService.Videos.Insert(video, "snippet,status", fileStream, GetMimeType(filePath));

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
            var credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId = "YOUR_CLIENT_ID",
                    ClientSecret = "YOUR_CLIENT_SECRET",
                },
                new[] { YouTubeService.Scope.YoutubeUpload },
                "user",
                CancellationToken.None,
                new FileDataStore(this.GetType().ToString())
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
}
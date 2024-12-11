using System.Globalization;
using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Options;
using Alex.YouTube.Joker.DomainServices.Services;
using Microsoft.Extensions.Logging;

namespace Alex.YouTube.Joker.DomainServices.Generators;

public class ZodiacGenerator : IZodiacGenerator
{
    private readonly IContentService _contentService;
    private readonly IVideoService _videoService;
    private readonly IYouTubeFacade _youTubeFacade;
    private readonly IChannelOptions _channelOptions;
    private readonly ILogger<JockerContentGenerator> _logger;

    public ZodiacGenerator(IContentService contentService, IVideoService videoService, IYouTubeFacade youTubeFacade,
        IChannelOptions channelOptions,
        ILogger<JockerContentGenerator> logger)
    {
        _contentService = contentService;
        _videoService = videoService;
        _youTubeFacade = youTubeFacade;
        _channelOptions = channelOptions;
        _logger = logger;
    }

    public async Task GenerateShorts(CancellationToken token)
    {
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));

        var zodiacs = await _contentService.GetZodiacs(tomorrow, token);
        
        foreach (var zodiacsChunk in zodiacs.Chunk(4))
        {
            var outputVideos = new List<string>();
            var seed = Random.Shared.NextInt64(100_000, 1_000_000);
            
            foreach (var joke in zodiacsChunk)
            {
                var output = Path.Combine(Path.GetTempPath(), $"joke_{seed}_{outputVideos.Count + 1}.mp4");

                await _videoService.CreateVideoWithXabe(new VideoRequest
                {
                    ImagePath = joke.ImagePath,
                    AudioPath = joke.AudioPath,
                    OutputVideoPath = output,
                    JokeText = joke.Text,
                    ThemeText = joke.Name
                }, token);

                outputVideos.Add(output);
                _logger.LogInformation("Video generated {joke}", joke.Text);
            }

            var outputFull = Path.Combine(Path.GetTempPath(), $"predict_{seed}.mp4");

            await _videoService.UnionVideos(outputVideos, outputFull, token);

            foreach (var uVideo in outputVideos)
            {
                if (File.Exists(uVideo))
                {
                    File.Delete(uVideo);
                }
            }

            await _youTubeFacade.UploadShort(new YouTubeShort
            {
                Title = $"Гороскоп на {tomorrow.ToString("D", new CultureInfo("ru-RU"))}",
                Description =
                    $"Гороскоп на {tomorrow.ToString("D", new CultureInfo("ru-RU"))}, для знаков: {string.Join(", ", zodiacsChunk.Select(s => s.Name))}",
                FilePath = outputFull,
                Tags = [$"Гороскоп, {string.Join(", ", zodiacsChunk.Select(s => s.Name))}"]
            }, _channelOptions.GetChannel("Oracle"), token);
        }
    }
}
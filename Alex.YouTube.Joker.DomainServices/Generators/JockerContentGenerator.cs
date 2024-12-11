using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Options;
using Alex.YouTube.Joker.DomainServices.Services;
using Microsoft.Extensions.Logging;

namespace Alex.YouTube.Joker.DomainServices.Generators;

public class JockerContentGenerator : IContentGenerator
{
    private readonly IContentService _contentService;
    private readonly IVideoService _videoService;
    private readonly IYouTubeFacade _youTubeFacade;
    private readonly IChannelOptions _channelOptions;
    private readonly ILogger<JockerContentGenerator> _logger;

    public JockerContentGenerator(IContentService contentService, IVideoService videoService, IYouTubeFacade youTubeFacade,
        IChannelOptions channelOptions,
        ILogger<JockerContentGenerator> logger)
    {
        _contentService = contentService;
        _videoService = videoService;
        _youTubeFacade = youTubeFacade;
        _channelOptions = channelOptions;
        _logger = logger;
    }

    public async Task GenerateShorts(string theme, CancellationToken token)
    {
        var jokes = await _contentService.GetJokesForShort(theme, token);

        _logger.LogInformation("Jokes created on theme, {theme}", theme);

        var outputVideos = new List<string>();

        var seed = Random.Shared.NextInt64(100_000, 1_000_000);

        foreach (var joke in jokes)
        {
            var output = Path.Combine(Path.GetTempPath(), $"joke_{seed}_{outputVideos.Count + 1}.mp4");

            await _videoService.CreateVideoWithXabe(new VideoRequest
            {
                ImagePath = joke.ImagePath,
                AudioPath = joke.AudioPath,
                OutputVideoPath = output,
                JokeText = joke.Text,
                ThemeText = joke.Theme
            }, token);

            outputVideos.Add(output);
            _logger.LogInformation("Video generated for joke {joke}", joke.Text);
        }

        var outputFull = Path.Combine(Path.GetTempPath(), $"joke_{seed}.mp4");

        await _videoService.UnionVideos(outputVideos, outputFull, token);

        foreach (var uVideo in outputVideos)
        {
            if (File.Exists(uVideo))
            {
                File.Delete(uVideo);
            }
        }

        _logger.LogInformation("Full video {outputFull} generated for joke {theme}", outputFull, theme);

        await _youTubeFacade.UploadShort(new YouTubeShort
        {
            Title = theme,
            Description = $"Анекдоты на тему: {theme}",
            FilePath = outputFull,
            Tags = ["Анекдоты"]
        }, _channelOptions.GetChannel("Jocker"), token);

        _logger.LogInformation("Video posted on theme, {theme}", theme);
    }
}
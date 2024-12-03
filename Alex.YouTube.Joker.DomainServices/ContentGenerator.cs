using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Services;
using Microsoft.Extensions.Logging;

namespace Alex.YouTube.Joker.DomainServices;

public class ContentGenerator : IContentGenerator
{
    private readonly IJokeService _jokeService;
    private readonly IVideoService _videoService;
    private readonly IYouTubeFacade _youTubeFacade;
    private readonly ILogger<ContentGenerator> _logger;

    public ContentGenerator(IJokeService jokeService, IVideoService videoService, IYouTubeFacade youTubeFacade, ILogger<ContentGenerator> logger)
    {
        _jokeService = jokeService;
        _videoService = videoService;
        _youTubeFacade = youTubeFacade;
        _logger = logger;
    }

    public async Task GenerateShorts(string theme, CancellationToken token)
    {
        var jokes = await _jokeService.GetJokesForShort(theme, token);

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
            _logger.LogInformation("Video generated for joke {joke}", joke);
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

        _logger.LogInformation("Full video generated for joke {theme}", theme);

        await _youTubeFacade.UploadShort(new YouTubeShort
        {
            Title = theme,
            Description = $"Анекдоты на тему: {theme}",
            FilePath = outputFull,
            Tags = ["Анекдоты"]
        }, token);

        _logger.LogInformation("Video posted on theme, {theme}", theme);
    }
}
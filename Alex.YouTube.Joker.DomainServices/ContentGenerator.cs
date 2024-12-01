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
        
        var output = $"C:\\Users\\Dunts\\youtube\\joke_{Random.Shared.NextInt64(100_000, 1_000_000)}.mp4";
        
        await _videoService.CreateVideoWithXabe(new VideoRequest
        {
            ImagePath = @"C:\Users\Dunts\youtube\oskar-smethurst-B1GtwanCbiw-unsplash.jpg",
            AudioPath = jokes,
            OutputVideoPath = output,
            JokeText = "",
            ThemeText = theme
        }, token);

        _logger.LogInformation("Video generated on theme, {theme}", theme);
        
        await _youTubeFacade.UploadShort(new YouTubeShort
        {
            Title = theme,
            Description = $"Анекдоты на теуму: {theme}",
            FilePath = output,
            Tags = ["Анекдоты"]
        }, token);
        
        _logger.LogInformation("Video posted on theme, {theme}", theme);
    }
}
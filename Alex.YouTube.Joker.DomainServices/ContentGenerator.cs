using Alex.YouTube.Joker.Domain;
using Alex.YouTube.Joker.DomainServices.Facades;
using Alex.YouTube.Joker.DomainServices.Services;

namespace Alex.YouTube.Joker.DomainServices;

public class ContentGenerator : IContentGenerator
{
    private readonly IJokeService _jokeService;
    private readonly IVideoService _videoService;

    public ContentGenerator(IJokeService jokeService, IVideoService videoService)
    {
        _jokeService = jokeService;
        _videoService = videoService;
    }

    public async Task<string> GetShort(string theme, CancellationToken token)
    {
        var jokes = await _jokeService.GetJokesForShort(theme, token);
        
        var output = "C:\\Users\\Dunts\\youtube\\joke.mp4";
        
        await _videoService.CreateVideoWithXabe(new VideoRequest
        {
            ImagePath = @"C:\Users\Dunts\youtube\oskar-smethurst-B1GtwanCbiw-unsplash.jpg",
            AudioPath = jokes,
            OutputVideoPath = output,
            JokeText = "",
            ThemeText = theme
        }, token);

        return output;
    }
}
using Alex.YouTube.Joker.Domain;
using Xabe.FFmpeg;

namespace Alex.YouTube.Joker.DomainServices.Services;

public class VideoService : IVideoService
{
    public async Task CreateVideoWithXabe(VideoRequest request, CancellationToken token)
    {
        // Установите путь к FFmpeg
        FFmpeg.SetExecutablesPath(@"C:\Users\Dunts\youtube\ffmpeg-master-latest-win64-gpl\bin");

        // Добавление изображения и аудио в видео
        var conversion = FFmpeg.Conversions.New()
            .AddParameter($"-loop 1 -i \"{request.ImagePath}\"") // Изображение
            .AddParameter($"-i \"{request.AudioPath}\"") // Аудио
            .SetOutput(request.OutputVideoPath)
            .AddParameter($"-vf \"drawtext=text='{request.ThemeText}':fontcolor=white:fontsize=48:x=(w-text_w)/2:y=50\"")
            .AddParameter($"-vf \"drawtext=text='{request.JokeText}':fontcolor=white:fontsize=36:x=(w-text_w)/2:y=h-100\"")
            .AddParameter("-s \"1080x1920\"") // Размер видео
            .AddParameter("-shortest"); // Длина видео = длина аудио

        await conversion.Start(token);
    }
}
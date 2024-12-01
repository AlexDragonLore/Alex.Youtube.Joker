using Alex.YouTube.Joker.Domain;
using Xabe.FFmpeg;

namespace Alex.YouTube.Joker.DomainServices.Services;

public class VideoService : IVideoService
{
    public async Task CreateVideoWithXabe(VideoRequest request, CancellationToken token)
    {
        // Установите путь к FFmpeg
        FFmpeg.SetExecutablesPath(@"C:\Users\Dunts\ffmpeg\bin");

        // Добавление изображения и аудио в видео
        var conversion = FFmpeg.Conversions.New()
            .AddParameter($"-loop 1 -i \"{request.ImagePath}\"") // Изображение
            .AddParameter($"-i \"{request.AudioPath}\"") // Аудио
            .SetOutput(request.OutputVideoPath)
            .AddParameter($"-vf \"drawtext=text='{request.ThemeText}':fontcolor=white:fontsize=48:x=(w-text_w)/2:y=50," +
                          $"drawtext=text='{request.JokeText}':fontcolor=white:fontsize=36:x=(w-text_w)/2:y=h-100\"")
            .AddParameter("-s 1080x1920") // Размер видео
            .AddParameter("-shortest"); // Длина видео = длина аудио

        await conversion.Start(token);
    }

    public async Task UnionVideos(IReadOnlyCollection<string> videoPaths, string outputPath, CancellationToken token)
    {
        if (videoPaths == null || !videoPaths.Any())
        {
            throw new ArgumentException("Список видеофайлов не может быть пустым.");
        }

        // Создаем временный файл для списка видео
        var tempFileListPath = Path.Combine(Path.GetTempPath(), "fileList.txt");
        await File.WriteAllLinesAsync(
            tempFileListPath, 
            videoPaths.Select(path => $"file '{path}'"), 
            token
        );

        try
        {
            // Настраиваем объединение видео
            var conversion = FFmpeg.Conversions.New()
                .AddParameter($"-f concat -safe 0 -i \"{tempFileListPath}\"")
                .SetOutput(outputPath)
                .AddParameter("-c copy"); // Без перекодирования

            // Запускаем процесс объединения
            await conversion.Start(token);
        }
        finally
        {
            // Удаляем временный файл
            if (File.Exists(tempFileListPath))
            {
                File.Delete(tempFileListPath);
            }
        }
    }
}
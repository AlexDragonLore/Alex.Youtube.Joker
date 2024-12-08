using Alex.YouTube.Joker.Domain;
using Xabe.FFmpeg;

namespace Alex.YouTube.Joker.DomainServices.Services;

public class VideoService : IVideoService
{
    public async Task CreateVideoWithXabe(VideoRequest request, CancellationToken token)
    {
        // Установите путь к FFmpeg
        FFmpeg.SetExecutablesPath("/usr/bin");

        // Создание видео из изображения и аудио
        var conversion = FFmpeg.Conversions.New()
            .AddParameter($"-loop 1 -i \"{request.ImagePath}\"") // Изображение
            .AddParameter($"-i \"{request.AudioPath}\"") // Аудио
            .SetOutput(request.OutputVideoPath)
            .AddParameter("-s 1080x1920") // Размер видео
            .AddParameter("-shortest") // Длина видео равна длине аудио
            .AddParameter("-c:v libx264") // Кодек H.264
            .AddParameter("-pix_fmt yuv420p") // Совместимость с видеопроигрывателями
            .AddParameter("-preset fast") // Баланс скорости и качества
            .AddParameter("-crf 23"); // Контроль качества

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
                .AddParameter("-c:v libx264") // Кодирование с использованием H.264
                .AddParameter("-preset fast") // Быстрая настройка кодека
                .AddParameter("-crf 23") // Контроль качества (меньше значение = выше качество)
                .AddParameter("-pix_fmt yuv420p"); // Совместимость с видеопроигрывателями

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
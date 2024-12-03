using SkiaSharp;

namespace Alex.YouTube.Joker.DomainServices.Services;

public class ImageService : IImageService
{
    public string GetRandomImageWithText(string jokeText)
    {
        // Укажите путь к директории с изображениями
        var directoryPath = Path.Combine(AppContext.BaseDirectory, "images");

        if (!Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException($"Директория не найдена: {directoryPath}");
        }

        // Получаем список всех файлов в директории
        var imageFiles = Directory.GetFiles(directoryPath, "*.jpg");

        if (imageFiles.Length == 0)
        {
            throw new Exception("В директории отсутствуют изображения формата .jpg.");
        }

        // Выбираем случайное изображение
        var random = new Random();
        var randomIndex = random.Next(imageFiles.Length);
        var selectedImage = imageFiles[randomIndex];

        // Копируем выбранное изображение во временную директорию
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.jpg");
        File.Copy(selectedImage, tempFilePath, true);

        return AddText(tempFilePath, jokeText);
    }

    private string AddText(string image, string jokeText)
    {
        // Загружаем изображение
        using var inputStream = File.OpenRead(image);
        using var bitmap = SKBitmap.Decode(inputStream);
        using var surface = SKSurface.Create(new SKImageInfo(bitmap.Width, bitmap.Height));
        var canvas = surface.Canvas;

        canvas.DrawBitmap(bitmap, 0, 0);

        // Настройки текста
        var fontSize = 80; // Размер шрифта
        var paint = new SKPaint
        {
            TextSize = fontSize,
            IsAntialias = true,
            Color = SKColors.White
        };

        var font = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), fontSize);
        var outlinePaint = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 8, // Увеличена толщина обводки для большей заметности
            Color = SKColors.Black,
            IsAntialias = true,
            TextSize = fontSize
        };

        // Определяем центр изображения
        var centerX = bitmap.Width / 2f;
        var centerY = bitmap.Height / 2f;

        // Область для текста
        var maxWidth = bitmap.Width - 40; // С учётом отступов
        var jokePosition = new SKPoint(centerX, centerY); // Текст размещается по центру

        // Рисуем текст с переносами
        DrawMultilineText(canvas, jokeText, jokePosition, maxWidth, paint, outlinePaint, 100, true);

        // Получаем изображение из поверхности
        using var snapshot = surface.Snapshot();

        // Кодируем изображение
        using var data = snapshot.Encode(SKEncodedImageFormat.Jpeg, 100);

        // Сохраняем изображение в файл
        using var outputStream = File.OpenWrite(image);
        data.SaveTo(outputStream);

        return image;
    }

    private void DrawMultilineText(SKCanvas canvas, string text, SKPoint startPosition, float maxWidth,
        SKPaint textPaint, SKPaint outlinePaint, float lineHeight, bool centerText = false)
    {
        var words = text.Split(' ');
        var lines = new List<string>();
        var currentLine = string.Empty;

        // Формируем строки с учётом ширины
        foreach (var word in words)
        {
            var testLine = string.IsNullOrEmpty(currentLine) ? word : $"{currentLine} {word}";

            if (textPaint.MeasureText(testLine) > maxWidth)
            {
                lines.Add(currentLine);
                currentLine = word;
            }
            else
            {
                currentLine = testLine;
            }
        }

        // Добавляем последнюю строку
        if (!string.IsNullOrEmpty(currentLine))
        {
            lines.Add(currentLine);
        }

        // Рассчитываем начальную позицию для центрирования по высоте
        var totalHeight = lines.Count * lineHeight;
        var yPosition = startPosition.Y - totalHeight / 2;

        // Рисуем строки
        foreach (var line in lines)
        {
            var xPosition = centerText
                ? startPosition.X - textPaint.MeasureText(line) / 2 // Центрирование по ширине
                : startPosition.X;

            // Рисуем обводку (черным)
            canvas.DrawText(line, xPosition, yPosition, outlinePaint);
            // Рисуем текст (белым)
            canvas.DrawText(line, xPosition, yPosition, textPaint);

            yPosition += lineHeight;
        }
    }
}
namespace Alex.YouTube.Joker.DomainServices.Services;

public class ImageService : IImageService
{
    public string GetRandomImage()
    {
        // Укажите путь к директории с изображениями
        var directoryPath = @"C:\Users\Dunts\youtube\images";

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

        return tempFilePath;
    }
}
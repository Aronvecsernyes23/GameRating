
using Microsoft.AspNetCore.Http;
using OrchardCore.Media;
using System.Text;

namespace DojoCourse.Module.Services;

public static class GameCoverStorage
{
    public static async Task<string> SaveAsync(
        IMediaFileStore mediaStore,
        IFormFile file)
    {
        const long maxSize = 5 * 1024 * 1024;

        if (file.Length == 0 || file.Length > maxSize)
        {
            throw new ArgumentException(
                "The image must be between 1 byte and 5 MB.");
        }

        string extension = Path.GetExtension(file.FileName)
            .ToLowerInvariant();

        if (extension is not
            (".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            throw new ArgumentException(
                "Only JPG, PNG and WebP files are allowed.");
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        byte[] bytes = stream.ToArray();

        bool jpeg = bytes.Length >= 3 &&
            bytes[0] == 0xFF &&
            bytes[1] == 0xD8 &&
            bytes[2] == 0xFF;

        bool png = bytes.Length >= 8 &&
            bytes[0] == 0x89 &&
            bytes[1] == 0x50 &&
            bytes[2] == 0x4E &&
            bytes[3] == 0x47 &&
            bytes[4] == 0x0D &&
            bytes[5] == 0x0A &&
            bytes[6] == 0x1A &&
            bytes[7] == 0x0A;

        bool webp = bytes.Length >= 12 &&
            Encoding.ASCII.GetString(bytes, 0, 4) == "RIFF" &&
            Encoding.ASCII.GetString(bytes, 8, 4) == "WEBP";

        bool valid = extension switch
        {
            ".jpg" or ".jpeg" => jpeg,
            ".png" => png,
            ".webp" => webp,
            _ => false
        };

        if (!valid)
        {
            throw new ArgumentException(
                "The file does not match its image format.");
        }

        stream.Position = 0;

        string path =
            $"game-submissions/{Guid.NewGuid():N}{extension}";

        await mediaStore.CreateFileFromStreamAsync(
            path, stream);

        return path;
    }
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Media;
using System.IO;
using System.Text.RegularExpressions;

namespace DojoCourse.Module.Controllers;

[Authorize(Roles = "Administrator")]
public class FileManagementController : Controller
{
    private readonly IMediaFileStore _mediaFileStore;

    private const long MaxFileSize = 5 * 1024 * 1024;

    public FileManagementController(
        IMediaFileStore mediaFileStore)
    {
        _mediaFileStore = mediaFileStore;
    }

    // A feltoltesi oldal megjelenitese
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    // Fajl feltoltese az Orchard Media Library-ba
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(IFormFile? file)
    {
        if (file == null || file.Length == 0)
        {
            ModelState.AddModelError(
                "", "Please select an image.");

            return View("Index");
        }

        if (file.Length > MaxFileSize)
        {
            ModelState.AddModelError(
                "", "Maximum image size is 5 MB.");

            return View("Index");
        }

        string extension = Path.GetExtension(
            file.FileName).ToLowerInvariant();

        if (extension is not
            (".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            ModelState.AddModelError(
                "", "Only JPG, PNG and WebP images are allowed.");

            return View("Index");
        }

        // A valodi fajltartalom ellenorzese
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        if (stream.Length == 0 || stream.Length > MaxFileSize)
        {
            ModelState.AddModelError(
                "", "Invalid image size.");

            return View("Index");
        }

        byte[] bytes = stream.ToArray();

        bool isJpeg =
            bytes.Length >= 3 &&
            bytes[0] == 0xFF &&
            bytes[1] == 0xD8 &&
            bytes[2] == 0xFF;

        bool isPng =
            bytes.Length >= 8 &&
            bytes.AsSpan(0, 8).SequenceEqual(
                new byte[] {
                    137, 80, 78, 71, 13, 10, 26, 10
                });

        bool isWebp =
            bytes.Length >= 12 &&
            bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
            bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8);

        bool validImage = extension switch
        {
            ".jpg" or ".jpeg" => isJpeg,
            ".png" => isPng,
            ".webp" => isWebp,
            _ => false
        };

        if (!validImage)
        {
            ModelState.AddModelError(
                "", "The file content is not a valid image type.");

            return View("Index");
        }

        // Soha nem hasznaljuk kozvetlenul
        // a feltolto altal adott fajlnevet.
        string storedFileName =
            $"dojo-cover-{Guid.NewGuid():N}{extension}";

        stream.Position = 0;

        string storedPath =
            await _mediaFileStore.CreateFileFromStreamAsync(
                storedFileName, stream);

        string publicUrl =
            _mediaFileStore.MapPathToPublicUrl(storedPath);

        TempData["UploadedImageUrl"] = publicUrl;
        TempData["UploadedFileName"] = storedFileName;

        return RedirectToAction(nameof(Index));
    }

    // A korabban mentett kep visszaolvasasa
    [HttpGet]
    public async Task<IActionResult> ReadFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !Regex.IsMatch(
                fileName,
                @"^dojo-cover-[a-f0-9]{32}\.(jpg|jpeg|png|webp)$"))
        {
            return BadRequest("Invalid file name.");
        }

        var fileInfo =
            await _mediaFileStore.GetFileInfoAsync(fileName);

        if (fileInfo == null || fileInfo.IsDirectory)
        {
            return NotFound();
        }

        Stream fileStream =
            await _mediaFileStore.GetFileStreamAsync(fileName);

        string extension =
            Path.GetExtension(fileName).ToLowerInvariant();

        string contentType = extension switch
        {
            ".png" => "image/png",
            ".webp" => "image/webp",
            _ => "image/jpeg"
        };

        return File(fileStream, contentType);
    }
}

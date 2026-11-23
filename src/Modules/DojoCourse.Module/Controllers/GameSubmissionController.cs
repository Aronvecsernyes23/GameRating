
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;

using DojoCourse.Module.Models;
using DojoCourse.Module.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using OrchardCore.ContentManagement;
using OrchardCore.Media;

namespace DojoCourse.Module.Controllers;

[Authorize]
public class GameSubmissionController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly IMediaFileStore _mediaFileStore;

    public GameSubmissionController(
        IContentManager contentManager,
        IMediaFileStore mediaFileStore)
    {
        _contentManager = contentManager;
        _mediaFileStore = mediaFileStore;
    }

    // A felhasználói űrlap megjelenítése.
    [HttpGet]
    public IActionResult Index()
    {
        return View(new GameSubmissionFormViewModel());
    }

    // Az űrlap feldolgozása.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> Create(
        GameSubmissionFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }

        // A borítókép mentése a Media Library tárhelyére.
        string coverPath = string.Empty;

        if (model.CoverImage != null)
        {
            try
            {
                coverPath = await SaveCoverAsync(
                    model.CoverImage);
            }
            catch (ArgumentException exception)
            {
                ModelState.AddModelError(
                    nameof(model.CoverImage),
                    exception.Message);

                return View("Index", model);
            }
        }

        // Új GameSuggestion létrehozása.
        var suggestion = await _contentManager.NewAsync(
            "GameSuggestion");

        suggestion.DisplayText = model.Title.Trim();
        suggestion.Owner = userId;

        // A TitlePart adata.
        suggestion.Content["TitlePart"] = new JsonObject
        {
            ["Title"] = suggestion.DisplayText
        };

        // A felhasználó által megadott játékadatok.
        var submission = new GameSubmissionData
        {
            Title = model.Title.Trim(),
            ReleaseDate = model.ReleaseDate,

            Developer = model.Developer.Trim(),
            Publisher = model.Publisher.Trim(),
            Platforms = model.Platforms.Trim(),
            Genre = model.Genre.Trim(),
            Description = model.Description.Trim(),

            CoverPath = coverPath,
            Status = "Pending",
            SubmittedByUserId = userId
        };

        suggestion.Content["GameSubmissionData"] =
            JsonSerializer.SerializeToNode(submission);

        // Piszkozatként mentjük.
        // Nem jelenik meg a nyilvános játéklistában.
        bool created = await _contentManager.CreateAsync(
            suggestion,
            VersionOptions.Draft);

        if (!created)
        {
            ModelState.AddModelError(
                string.Empty,
                "The game suggestion could not be saved.");

            return View("Index", model);
        }

        return RedirectToAction(nameof(Submitted));
    }

    // Sikeres beküldés visszajelzése.
    [HttpGet]
    public IActionResult Submitted()
    {
        return View();
    }

    // Biztonságos borítókép-feltöltés.
    private async Task<string> SaveCoverAsync(
        IFormFile file)
    {
        const long maxSize = 5 * 1024 * 1024;

        if (file.Length == 0 || file.Length > maxSize)
        {
            throw new ArgumentException(
                "The cover image must be smaller than 5 MB.");
        }

        string extension =
            Path.GetExtension(file.FileName)
                .ToLowerInvariant();

        if (extension is not
            (".jpg" or ".jpeg" or ".png" or ".webp"))
        {
            throw new ArgumentException(
                "Only JPG, PNG and WebP images are allowed.");
        }

        // Maximum 5 MB-ot töltünk memóriába.
        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);

        byte[] header = stream.ToArray();

        bool isJpeg =
            header.Length >= 3 &&
            header[0] == 0xFF &&
            header[1] == 0xD8 &&
            header[2] == 0xFF;

        bool isPng =
            header.Length >= 8 &&
            header[0] == 0x89 &&
            header[1] == 0x50 &&
            header[2] == 0x4E &&
            header[3] == 0x47 &&
            header[4] == 0x0D &&
            header[5] == 0x0A &&
            header[6] == 0x1A &&
            header[7] == 0x0A;

        bool isWebp =
            header.Length >= 12 &&
            System.Text.Encoding.ASCII.GetString(
                header, 0, 4) == "RIFF" &&
            System.Text.Encoding.ASCII.GetString(
                header, 8, 4) == "WEBP";

        bool isValid = extension switch
        {
            ".jpg" or ".jpeg" => isJpeg,
            ".png" => isPng,
            ".webp" => isWebp,
            _ => false
        };

        if (!isValid)
        {
            throw new ArgumentException(
                "The uploaded file is not a valid image format.");
        }

        stream.Position = 0;

        // Nem használjuk a felhasználó eredeti fájlnevét.
        string path =
            $"game-submissions/{Guid.NewGuid():N}{extension}";

        await _mediaFileStore.CreateFileFromStreamAsync(
            path,
            stream);

        return path;
    }
}

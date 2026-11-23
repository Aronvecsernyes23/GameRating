
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

using DojoCourse.Module.Models;
using DojoCourse.Module.Services;
using DojoCourse.Module.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents;
using OrchardCore.Media;

using YesSql;

namespace DojoCourse.Module.Controllers;

[Admin("DojoCourse/GameModeration/{action}/{id?}")]
[Authorize(Roles = "Administrator,GameModerator")]
public class AdminController : Controller
{
    private readonly IContentManager _contentManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISession _session;
    private readonly IMediaFileStore _mediaFileStore;

    public AdminController(
        IContentManager contentManager,
        IAuthorizationService authorizationService,
        ISession session,
        IMediaFileStore mediaFileStore)
    {
        _contentManager = contentManager;
        _authorizationService = authorizationService;
        _session = session;
        _mediaFileStore = mediaFileStore;
    }

    // --------------------------------------------------
    // Game Moderation lista
    // --------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = new GameModerationViewModel();

        var suggestions = await _session
            .Query<ContentItem, ContentItemIndex>(
                x => x.ContentType == "GameSuggestion" && x.Latest)
            .ListAsync();

        foreach (var item in suggestions)
        {
            if (!await CanEditAsync(item))
                continue;

            var data = ReadData(item);

            model.Suggestions.Add(new GameSuggestionRowViewModel
            {
                ContentItemId = item.ContentItemId,
                Title = item.DisplayText,
                Published = item.Published,
                Status = data?.Status ?? "Legacy"
            });
        }

        return View(model);
    }

    // --------------------------------------------------
    // A 13. ora jogosultsagtesztje
    // --------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> ContentAuthorization(string? id)
    {
        ContentItem? suggestion;

        bool isExisting = !string.IsNullOrWhiteSpace(id);

        if (isExisting)
        {
            suggestion = await LoadSuggestionAsync(id!);

            if (suggestion == null)
                return NotFound();
        }
        else
        {
            suggestion = await _contentManager.NewAsync(
                "GameSuggestion");
        }

        bool canEdit = await CanEditAsync(suggestion);

        return View(new GameSuggestionAuthorizationViewModel
        {
            Title = isExisting
                ? suggestion.DisplayText
                : "New Game Suggestion",

            IsExisting = isExisting,
            CanEdit = canEdit
        });
    }

    // --------------------------------------------------
    // Reszletek megtekintese
    // --------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Details(string id)
    {
        var item = await LoadSuggestionAsync(id);

        if (item == null)
            return NotFound();

        if (!await CanEditAsync(item))
            return Forbid();

        var data = ReadData(item);

        string? gameUrl = null;

        if (!string.IsNullOrWhiteSpace(data?.ApprovedGameId))
        {
            var publishedGame = await _contentManager.GetAsync(
                data.ApprovedGameId,
                VersionOptions.Published);

            if (publishedGame != null)
            {
                string? path = publishedGame.Content
                    ["AutoroutePart"]?["Path"]?.ToString();

                if (!string.IsNullOrWhiteSpace(path))
                    gameUrl = Url.Content("~/" + path.TrimStart('/'));
            }
        }

        return View(new GameSuggestionDetailsViewModel
        {
            Id = item.ContentItemId,
            Title = item.DisplayText,
            Data = data,
            CoverUrl = GetCoverUrl(data?.CoverPath),
            GameUrl = gameUrl
        });
    }

    // --------------------------------------------------
    // Szerkeszto oldal megnyitasa
    // --------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var item = await LoadSuggestionAsync(id);

        if (item == null)
            return NotFound();

        if (!await CanEditAsync(item))
            return Forbid();

        var data = ReadData(item);

        if (data == null || !IsPending(data))
            return BadRequest("Only pending submissions can be edited.");

        return View(new EditGameSuggestionViewModel
        {
            Id = item.ContentItemId,
            Title = data.Title,
            ReleaseDate = data.ReleaseDate,
            Developer = data.Developer,
            Publisher = data.Publisher,
            Platforms = data.Platforms,
            Genre = data.Genre,
            Description = data.Description,
            CurrentCoverUrl = GetCoverUrl(data.CoverPath)
        });
    }

    // --------------------------------------------------
    // Szerkesztett adatok mentese
    // --------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(6_000_000)]
    public async Task<IActionResult> Edit(
        EditGameSuggestionViewModel model)
    {
        var item = await LoadSuggestionAsync(model.Id);

        if (item == null)
            return NotFound();

        if (!await CanEditAsync(item))
            return Forbid();

        var data = ReadData(item);

        if (data == null || !IsPending(data))
            return BadRequest("Submission is not pending.");

        model.CurrentCoverUrl = GetCoverUrl(data.CoverPath);

        if (!ModelState.IsValid)
            return View(model);

        if (model.CoverImage != null)
        {
            try
            {
                data.CoverPath = await GameCoverStorage.SaveAsync(
                    _mediaFileStore,
                    model.CoverImage);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(
                    nameof(model.CoverImage), ex.Message);

                return View(model);
            }
        }

        data.Title = model.Title.Trim();
        data.ReleaseDate = model.ReleaseDate;
        data.Developer = model.Developer.Trim();
        data.Publisher = model.Publisher.Trim();
        data.Platforms = model.Platforms.Trim();
        data.Genre = model.Genre.Trim();
        data.Description = model.Description.Trim();

        item.DisplayText = data.Title;
        item.Content["TitlePart"] = new JsonObject
        {
            ["Title"] = data.Title
        };

        WriteData(item, data);
        await _contentManager.UpdateAsync(item);

        TempData["Success"] = "Game suggestion updated successfully.";

        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    // --------------------------------------------------
    // Elutasitas
    // --------------------------------------------------
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(
        string id,
        string reason)
    {
        var item = await LoadSuggestionAsync(id);

        if (item == null)
            return NotFound();

        if (!await CanEditAsync(item))
            return Forbid();

        var data = ReadData(item);

        if (data == null || !IsPending(data))
            return BadRequest("Submission is not pending.");

        reason = reason?.Trim() ?? string.Empty;

        if (reason.Length < 5 || reason.Length > 500)
        {
            TempData["Error"] =
                "Please enter a reason between 5 and 500 characters.";

            return RedirectToAction(nameof(Details), new { id });
        }

        data.Status = "Rejected";
        data.ReviewReason = reason;

        WriteData(item, data);
        await _contentManager.UpdateAsync(item);

        TempData["Success"] = "Game suggestion rejected.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // --------------------------------------------------
    // Elfogadas: GameSuggestion -> Game
    // --------------------------------------------------
    [Authorize(Roles = "Administrator")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(string id)
    {
        var suggestion = await LoadSuggestionAsync(id);

        if (suggestion == null)
            return NotFound();

        if (!await CanEditAsync(suggestion))
            return Forbid();

        var data = ReadData(suggestion);

        if (data == null)
            return BadRequest("No structured submission data.");

        if (!IsPending(data))
            return BadRequest("Submission is not pending.");

        // Ha egy korabbi probalkozas mar letrehozott
        // egy Game tartalmat, ugyanazt hasznaljuk.
        ContentItem? game = null;

        if (!string.IsNullOrWhiteSpace(data.ApprovedGameId))
        {
            game = await _contentManager.GetAsync(
                data.ApprovedGameId, VersionOptions.Latest);

            if (game == null || game.ContentType != "Game")
            {
                TempData["Error"] =
                    "The linked Game could not be found. " +
                    "Check the stored ApprovedGameId.";

                return RedirectToAction(nameof(Details), new { id });
            }
        }
        else
        {
            game = await _contentManager.NewAsync("Game");

            game.DisplayText = data.Title;

            game.Content["TitlePart"] = new JsonObject
            {
                ["Title"] = data.Title
            };

            // Egyedi URL a javaslat azonositojaval.
            string slug = Regex.Replace(
                data.Title.ToLowerInvariant(),
                "[^a-z0-9]+", "-").Trim('-');

            if (string.IsNullOrEmpty(slug))
                slug = "game";

            if (slug.Length > 80)
                slug = slug[..80].Trim('-');

            string suffix = Regex.Replace(
                suggestion.ContentItemId, "[^a-zA-Z0-9]", "");

            if (suffix.Length > 10)
                suffix = suffix[..10];

            game.Content["AutoroutePart"] = new JsonObject
            {
                ["Path"] = $"games/{slug}-{suffix}"
            };

            var paths = new JsonArray();

            if (!string.IsNullOrWhiteSpace(data.CoverPath))
                paths.Add(data.CoverPath);

            // A Game Content Type meglevo mezoi.
            game.Content["Game"] = new JsonObject
            {
                ["Developer"] = new JsonObject
                {
                    ["Text"] = data.Developer
                },

                ["Publisher"] = new JsonObject
                {
                    ["Text"] = data.Publisher
                },

                ["Platforms"] = new JsonObject
                {
                    ["Text"] = data.Platforms
                },

                ["ReleaseDate"] = new JsonObject
                {
                    ["Value"] = data.ReleaseDate.HasValue
                        ? JsonValue.Create(
                            data.ReleaseDate.Value.ToDateTime(
                                TimeOnly.MinValue))
                        : null
                },

                ["CoverImage"] = new JsonObject
                {
                    ["Paths"] = paths
                },

                ["GameGenreText"] = new JsonObject
                {
                    ["Text"] = data.Genre
                },

                ["GameDescription"] = new JsonObject
                {
                    ["Text"] = data.Description
                }
            };

            // Eloszor piszkozatkent hozzuk letre.
            bool created = await _contentManager.CreateAsync(
                game,
                VersionOptions.Draft);

            if (!created)
            {
                TempData["Error"] =
                    "The Game content could not be created.";

                return RedirectToAction(nameof(Details), new { id });
            }

            // Megorizzuk az azonositojat.
            // Ujraprobalkozaskor nem keszitunk uj Game-et.
            data.ApprovedGameId = game.ContentItemId;

            WriteData(suggestion, data);
            await _contentManager.UpdateAsync(suggestion);
        }

        // A draft Game publikalasa.
        if (!game.Published)
        {
            bool published = await _contentManager.PublishAsync(game);

            if (!published)
            {
                TempData["Error"] =
                    "The Game was created as a draft, " +
                    "but publication failed. You may retry.";

                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Csak sikeres publikalas utan Approved.
        data.Status = "Approved";
        data.ReviewReason = string.Empty;

        WriteData(suggestion, data);
        await _contentManager.UpdateAsync(suggestion);

        TempData["Success"] =
            "Game suggestion approved and Game published!";

        return RedirectToAction(nameof(Details), new { id });
    }

    // --------------------------------------------------
    // Segedmetodusok
    // --------------------------------------------------
    private async Task<ContentItem?> LoadSuggestionAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return null;

        var item = await _contentManager.GetAsync(
            id, VersionOptions.Latest);

        return item?.ContentType == "GameSuggestion"
            ? item
            : null;
    }

    private Task<bool> CanEditAsync(ContentItem item)
    {
        return _authorizationService.AuthorizeAsync(
            User,
            CommonPermissions.EditContent,
            item);
    }


    private static GameSubmissionData? ReadData(ContentItem item)
    {
        JsonNode? json = item.Content["GameSubmissionData"];

        if (json == null)
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<GameSubmissionData>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }


    private static void WriteData(
        ContentItem item,
        GameSubmissionData data)
    {
        item.Content["GameSubmissionData"] =
            JsonSerializer.SerializeToNode(data);
    }

    private static bool IsPending(GameSubmissionData data)
    {
        return string.Equals(
            data.Status,
            "Pending",
            StringComparison.OrdinalIgnoreCase);
    }

    private string? GetCoverUrl(string? coverPath)
    {
        if (string.IsNullOrWhiteSpace(coverPath))
            return null;

        return _mediaFileStore.MapPathToPublicUrl(coverPath);
    }
}

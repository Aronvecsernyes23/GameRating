
using DojoCourse.Module.Indexes;
using DojoCourse.Module.ViewModels;
using System.Text.Json.Nodes;

using Microsoft.AspNetCore.Mvc;

using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Media;
using OrchardCore.Media.Fields;
using OrchardCore.Modules;

using OrchardCore.ContentFields.Fields;

using YesSql;

namespace DojoCourse.Module.Controllers;

public class GamesController : Controller
{
    private readonly ISession _session;
    private readonly IContentManager _contentManager;
    private readonly IMediaFileStore _mediaFileStore;
    private readonly IClock _clock;

    public GamesController(
        ISession session,
        IContentManager contentManager,
        IMediaFileStore mediaFileStore,
        IClock clock)
    {
        _session = session;
        _contentManager = contentManager;
        _mediaFileStore = mediaFileStore;
        _clock = clock;
    }

    // Összes publikált játék vagy fejlesztő szerinti szűrés.
    [HttpGet]
    public async Task<IActionResult> List(string? developer)
    {
        developer = developer?.Trim();


        var games = (await _session
            .Query<ContentItem, ContentItemIndex>(
                index => index.ContentType == "Game"
                && index.Published)
            .ListAsync())
            .ToList();

        // Fejleszto szerinti szures
        if (!string.IsNullOrWhiteSpace(developer))
        {
            string searchDeveloper = developer.Trim();

            games = games
                .Where(game =>
                {
                    string gameDeveloper =
                        game.Content["Game"]?
                            ["Developer"]?
                            ["Text"]?
                            .ToString()?
                            .Trim() ?? string.Empty;

                    return string.Equals(
                        gameDeveloper,
                        searchDeveloper,
                        StringComparison.OrdinalIgnoreCase);
                })
                .ToList();
        }

        var model = new GamesListViewModel
        {
            DeveloperFilter = developer,
            GeneratedUtc = _clock.UtcNow
        };


        foreach (var game in games.OrderBy(
            g => g.DisplayText,
            StringComparer.OrdinalIgnoreCase))
        {
            // A Game tartalomtípus mezőinek elérése.
            var gamePart = game.Get<ContentPart>("Game");

            // TextField mezők kiolvasása.
            string developerName =
                gamePart?.Get<TextField>("Developer")?.Text
                ?? string.Empty;

            string publisherName =
                gamePart?.Get<TextField>("Publisher")?.Text
                ?? string.Empty;

            // Borítókép kiolvasása.
            var coverField =
                gamePart?.Get<MediaField>("CoverImage");

            string? coverPath = coverField?.Paths?.FirstOrDefault();

            string? coverUrl = null;

            if (!string.IsNullOrWhiteSpace(coverPath))
            {
                coverUrl =
                    _mediaFileStore.MapPathToPublicUrl(coverPath);
            }

            // A játék eredeti Autoroute URL-je
            string? gamePath = game.Content["AutoroutePart"]?
                ["Path"]?.ToString();

            string gameUrl = string.IsNullOrWhiteSpace(gamePath)
                ? string.Empty
                : Url.Content("~/" + gamePath.TrimStart('/'));

            model.Games.Add(new GameListItemViewModel
            {
                ContentItemId = game.ContentItemId,
                Title = game.DisplayText,
                Developer = developerName,
                Publisher = publisherName,
                CoverUrl = coverUrl,
                GameUrl = gameUrl
            });

        }


        return View(model);
    }

    // Megnyitjuk a meglévő játékadatlapot.
    [HttpGet]
    public async Task<IActionResult> Open(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        // IContentManager: publikált tartalom betöltése.
        var game = await _contentManager.GetAsync(
            id,
            VersionOptions.Published);

        if (game == null || game.ContentType != "Game")
        {
            return NotFound();
        }

        // A meglévő Autoroute útvonalat használjuk.
        JsonNode? autoroute =
            game.Content["AutoroutePart"] as JsonNode;

        string? path =
            (autoroute?["Path"] as JsonValue)
                ?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(path))
        {
            return NotFound();
        }

        return LocalRedirect(
            Url.Content("~/" + path.TrimStart('/')));
    }
}

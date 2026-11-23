
using System.Security.Claims;

using DojoCourse.Module.Services;
using DojoCourse.Module.ViewModels;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using OrchardCore.ContentManagement;

namespace DojoCourse.Module.Controllers;

public class RatingController : Controller
{
    private readonly GameRatingService _ratingService;
    private readonly IContentManager _contentManager;

    public RatingController(
        GameRatingService ratingService,
        IContentManager contentManager)
    {
        _ratingService = ratingService;
        _contentManager = contentManager;
    }

    // A játék értékelőoldala.
    [HttpGet]
    public async Task<IActionResult> Index(
        string gameId,
        bool saved = false)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            return BadRequest();
        }

        // Kizárólag publikált játék értékelhető.
        var game = await _contentManager.GetAsync(
            gameId,
            VersionOptions.Published);

        if (game == null || game.ContentType != "Game")
        {
            return NotFound();
        }

        // A felhasználó stabil Orchard-azonosítója.
        string? userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        var summary = await _ratingService.GetSummaryAsync(
            gameId,
            userId);

        string? path = game.Content["AutoroutePart"]?
            ["Path"]?.ToString();

        string gameUrl = string.IsNullOrWhiteSpace(path)
            ? Url.Content("~/")
            : Url.Content("~/" + path.TrimStart('/'));

        var model = new RatingPageViewModel
        {
            GameContentItemId = game.ContentItemId,
            GameTitle = game.DisplayText,
            GameUrl = gameUrl,
            RatingCount = summary.Count,
            AverageRating = summary.Average,
            YourScore = summary.YourScore,
            IsAuthenticated =
                User.Identity?.IsAuthenticated == true,
            Saved = saved
        };

        return View(model);
    }


    [HttpGet]
    [AllowAnonymous]
    [ResponseCache(
        NoStore = true,
        Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> Summary(string gameId)
    {
        if (string.IsNullOrWhiteSpace(gameId))
        {
            return BadRequest();
        }

        // Ellenőrizzük, hogy létező, publikált játék-e.
        var game = await _contentManager.GetAsync(
            gameId,
            VersionOptions.Published);

        if (game == null || game.ContentType != "Game")
        {
            return NotFound();
        }

        // A saját értékelést csak a bejelentkezett
        // felhasználóhoz kötjük.
        string? userId = User.Identity?.IsAuthenticated == true
            ? User.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;

        // A korábban elkészített GameRatingService.
        var summary = await _ratingService.GetSummaryAsync(
            gameId,
            userId);

        return Json(new
        {
            average = summary.Average,
            count = summary.Count,
            yourScore = summary.YourScore
        });
    }


    // Értékelés mentése - csak bejelentkezve.
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(
        string gameContentItemId,
        int score)
    {
        if (string.IsNullOrWhiteSpace(gameContentItemId))
        {
            return BadRequest();
        }

        if (score < 1 || score > 10)
        {
            return BadRequest(
                "Rating must be between 1 and 10.");
        }

        var game = await _contentManager.GetAsync(
            gameContentItemId,
            VersionOptions.Published);

        if (game == null || game.ContentType != "Game")
        {
            return NotFound();
        }

        string? userId =
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Challenge();
        }


        await _ratingService.SaveOrUpdateAsync(
            gameContentItemId,
            userId,
            score);

        string? gamePath = game.Content["AutoroutePart"]?
            ["Path"]?.ToString();

        if (string.IsNullOrWhiteSpace(gamePath))
        {
            return RedirectToAction(
                nameof(Index),
                "Rating",
                new
                {
                    area = "DojoCourse.Module",
                    gameId = gameContentItemId,
                    saved = true
                });
        }

        return LocalRedirect(
            Url.Content("~/" + gamePath.TrimStart('/')));

    }
}

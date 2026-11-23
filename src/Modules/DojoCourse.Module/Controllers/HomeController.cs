
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Notify;

namespace DojoCourse.Module.Controllers;

public class HomeController : Controller
{
    private readonly IStringLocalizer<HomeController> _textLocalizer;
    private readonly IHtmlLocalizer<HomeController> _htmlLocalizer;
    private readonly INotifier _notifier;
    private readonly ILogger<HomeController> _logger;

    public HomeController(
        IStringLocalizer<HomeController> textLocalizer,
        IHtmlLocalizer<HomeController> htmlLocalizer,
        INotifier notifier,
        ILogger<HomeController> logger)
    {
        _textLocalizer = textLocalizer;
        _htmlLocalizer = htmlLocalizer;
        _notifier = notifier;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        _logger.LogInformation(
            "The Dojo Course module page was opened.");

        ViewData["WelcomeMessage"] =
            _textLocalizer[
                "Welcome to the Dojo Course game database!"
            ].Value;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Notify()
    {
        _logger.LogInformation(
            "A user tested the module notification.");

        await _notifier.SuccessAsync(
            _htmlLocalizer[
                "The Dojo Course module is working successfully!"
            ]);

        return RedirectToAction(nameof(Index));
    }
}

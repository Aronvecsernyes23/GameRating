
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;

using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Users;
using OrchardCore.Users.Events;

namespace DojoCourse.Module.Events;

public class LoginGreeting : LoginFormEventBase
{
    private readonly INotifier _notifier;
    private readonly IHtmlLocalizer<LoginGreeting> _htmlLocalizer;
    private readonly ILogger<LoginGreeting> _logger;

    public LoginGreeting(
        INotifier notifier,
        IHtmlLocalizer<LoginGreeting> htmlLocalizer,
        ILogger<LoginGreeting> logger)
    {
        _notifier = notifier;
        _htmlLocalizer = htmlLocalizer;
        _logger = logger;
    }

    // Sikeres bejelentkezés után fut le.
    public override async Task LoggedInAsync(IUser user)
    {
        _logger.LogInformation(
            "Dojo Course: successful login event handled.");

        await _notifier.SuccessAsync(
            _htmlLocalizer[
                "Welcome back, {0}! Explore games and share a game suggestion.",
                user.UserName
            ]);
    }
}

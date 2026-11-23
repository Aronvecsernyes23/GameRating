
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace DojoCourse.Module.Navigation;

public sealed class AdminMenu : INavigationProvider
{
    private readonly IStringLocalizer<AdminMenu> _localizer;

    public AdminMenu(
        IStringLocalizer<AdminMenu> localizer)
    {
        _localizer = localizer;
    }

    public ValueTask BuildNavigationAsync(
        string name,
        NavigationBuilder builder)
    {
        // Csak az Orchard admin menüjét bővítjük.
        if (!string.Equals(
            name,
            "admin",
            StringComparison.OrdinalIgnoreCase))
        {
            return ValueTask.CompletedTask;
        }

        builder.Add(
            _localizer["Dojo Course"],
            "10",
            menu => menu
                .Add(
                    _localizer["Game Moderation"],
                    "1",
                    item => item.Url(
                        "/Admin/DojoCourse/GameModeration/Index")
                )
                .Add(
                    _localizer["Content Authorization Test"],
                    "2",
                    item => item.Url(
                        "/Admin/DojoCourse/GameModeration/ContentAuthorization")
                )
        );

        return ValueTask.CompletedTask;
    }
}

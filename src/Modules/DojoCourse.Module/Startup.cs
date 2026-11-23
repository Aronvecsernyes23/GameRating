using DojoCourse.Module.Drivers;
using DojoCourse.Module.Events;
using DojoCourse.Module.Filters;
using DojoCourse.Module.Handlers;
using DojoCourse.Module.Indexes;
using DojoCourse.Module.Migrations;
using DojoCourse.Module.Models;
using DojoCourse.Module.Navigation;
using DojoCourse.Module.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Media;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Users.Events;

namespace DojoCourse.Module;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddContentPart<GameTipPart>()
            .UseDisplayDriver<GameTipPartDisplayDriver>()
            .AddHandler<GameTipPartHandler>();

        services.AddDataMigration<GameTipMigration>();

        services.AddIndexProvider<GameDetailsIndexProvider>();

        services.AddDataMigration<GameDetailsMigration>();

        services.AddScoped<ShapeInjectingFilter>();

        services.Configure<MvcOptions>(options =>
        {
            options.Filters.AddService<ShapeInjectingFilter>();
        }); 

        services.AddScoped<ILoginFormEvent, LoginGreeting>();
        services.AddNavigationProvider<DojoCourse.Module.Navigation.AdminMenu>();

        services.AddIndexProvider<GameRatingIndexProvider>();
        services.AddDataMigration<GameRatingMigration>();

        // Game Rating Service
        services.AddScoped<GameRatingService>();

    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        routes.MapAreaControllerRoute(
            name: "Home",
            areaName: "DojoCourse.Module",
            pattern: "Home/Index",
            defaults: new { controller = "Home", action = "Index" }
        );
    }
}



using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Layout;

namespace DojoCourse.Module.Filters;

public class ShapeInjectingFilter : IAsyncResultFilter
{
    private readonly ILayoutAccessor _layoutAccessor;
    private readonly IShapeFactory _shapeFactory;

    public ShapeInjectingFilter(
        ILayoutAccessor layoutAccessor,
        IShapeFactory shapeFactory)
    {
        _layoutAccessor = layoutAccessor;
        _shapeFactory = shapeFactory;
    }

    public async Task OnResultExecutionAsync(
        ResultExecutingContext context,
        ResultExecutionDelegate next)
    {
        // Csak MVC View vagy Razor Page eredmenyeknel.
        if (context.Result is not ViewResult &&
            context.Result is not PageResult)
        {
            await next();
            return;
        }

        // Az aktualis route adatainak lekerese.
        string? area =
            context.RouteData.Values["area"]?.ToString();

        string? controller =
            context.RouteData.Values["controller"]?.ToString();

        string? action =
            context.RouteData.Values["action"]?.ToString();

        // Csak a sajat GamesController List oldalan fusson.
        bool isGamesList =
            string.Equals(
                area,
                "DojoCourse.Module",
                StringComparison.OrdinalIgnoreCase)
            &&
            string.Equals(
                controller,
                "Games",
                StringComparison.OrdinalIgnoreCase)
            &&
            string.Equals(
                action,
                "List",
                StringComparison.OrdinalIgnoreCase);

        if (!isGamesList)
        {
            await next();
            return;
        }

        // Az Orchard Core Layout shape lekerese.
        dynamic layout =
            await _layoutAccessor.GetLayoutAsync();

        // Sajat Shape letrehozasa.
        var shape =
            await _shapeFactory.CreateAsync(
                "LayoutInjectedShape");

        // Shape elhelyezese egy sajat Layout zonaban.
        await layout.GameListBanner.AddAsync(
            shape, "10");

        // A View megjelenitese ezutan kovetkezik.
        await next();
    }
}

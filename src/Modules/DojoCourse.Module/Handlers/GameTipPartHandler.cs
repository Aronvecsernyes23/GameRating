
using DojoCourse.Module.Models;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

namespace DojoCourse.Module.Handlers;

public class GameTipPartHandler
    : ContentPartHandler<GameTipPart>
{
    private readonly ILogger<GameTipPartHandler> _logger;

    public GameTipPartHandler(
        ILogger<GameTipPartHandler> logger)
    {
        _logger = logger;
    }

    public override Task CreatingAsync(
        CreateContentContext context,
        GameTipPart part)
    {
        GeneratePreview(part);

        _logger.LogInformation(
            "Creating Game Tip: {ContentItemId}",
            context.ContentItem.ContentItemId);

        return Task.CompletedTask;
    }

    public override Task UpdatingAsync(
        UpdateContentContext context,
        GameTipPart part)
    {
        GeneratePreview(part);

        _logger.LogInformation(
            "Updating Game Tip: {ContentItemId}",
            context.ContentItem.ContentItemId);

        return Task.CompletedTask;
    }


    private static void GeneratePreview(GameTipPart part)
    {
        string text = part.TipText?.Text ?? string.Empty;

        text = string.Join(
            " ",
            text.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries));

        const int maxLength = 120;

        if (text.Length > maxLength)
        {
            text = text[..117].TrimEnd() + "...";
        }

        part.PreviewText = text;

        part.Apply();
    }

}

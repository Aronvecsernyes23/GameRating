
using DojoCourse.Module.Models;
using DojoCourse.Module.ViewModels;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;

namespace DojoCourse.Module.Drivers;

public class GameTipPartDisplayDriver
    : ContentPartDisplayDriver<GameTipPart>
{
    // A Game Tip megjelenítése
    public override IDisplayResult Display(
        GameTipPart part,
        BuildPartDisplayContext context)
    {
        return Initialize<GameTipPartViewModel>(
            "GameTipPart",
            model => PopulateViewModel(part, model))
            .Location("Detail", "Content:10")
            .Location("Summary", "Content:10");
    }

    // Admin szerkesztőfelület létrehozása
    public override IDisplayResult Edit(
        GameTipPart part,
        BuildPartEditorContext context)
    {
        return Initialize<GameTipPartViewModel>(
            "GameTipPart_Edit",
            model => PopulateViewModel(part, model))
            .Location("Content:10");
    }

    // A szerkesztett adatok mentése
    public override async Task<IDisplayResult> UpdateAsync(
        GameTipPart part,
        UpdatePartEditorContext context)
    {
        var viewModel = new GameTipPartViewModel();

        if (await context.Updater.TryUpdateModelAsync(
            viewModel, Prefix))
        {
            part.GameName ??= new TextField();
            part.TipText ??= new TextField();
            part.Difficulty ??= new TextField();

            part.GameName.Text = viewModel.GameName.Trim();
            part.TipText.Text = viewModel.TipText.Trim();
            part.Difficulty.Text = viewModel.Difficulty;
        }

        return Edit(part, context);
    }

    // A Content Part adatait átmásoljuk
    // a ViewModel tulajdonságaiba
    private static void PopulateViewModel(
        GameTipPart part,
        GameTipPartViewModel model)
    {
        model.GameName =
            part.GameName?.Text ?? string.Empty;

        model.TipText =
            part.TipText?.Text ?? string.Empty;

        model.Difficulty =
            part.Difficulty?.Text ?? string.Empty;

        model.PreviewText = part.PreviewText ?? string.Empty;
    }
}

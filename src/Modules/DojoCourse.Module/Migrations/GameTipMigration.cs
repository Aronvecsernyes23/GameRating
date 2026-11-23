
using System.Threading.Tasks;
using DojoCourse.Module.Models;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace DojoCourse.Module.Migrations;

public class GameTipMigration : DataMigration
{
    private readonly IContentDefinitionManager
        _contentDefinitionManager;

    public GameTipMigration(
        IContentDefinitionManager contentDefinitionManager)
    {
        _contentDefinitionManager = contentDefinitionManager;
    }

    public async Task<int> CreateAsync()
    {

        await _contentDefinitionManager.AlterPartDefinitionAsync(
            nameof(GameTipPart),
            part => part

                .Attachable()

                .WithDisplayName("Game Tip Details")

                .WithField(
                    nameof(GameTipPart.GameName),
                    field => field
                        .OfType(nameof(TextField))
                        .WithDisplayName("Game Name")
                )

                .WithField(
                    nameof(GameTipPart.TipText),
                    field => field
                        .OfType(nameof(TextField))
                        .WithDisplayName("Tip Text")
                )

                .WithField(
                    nameof(GameTipPart.Difficulty),
                    field => field
                        .OfType(nameof(TextField))
                        .WithDisplayName("Difficulty")
                )
        );


        await _contentDefinitionManager.AlterTypeDefinitionAsync(
            "GameTip",
            type => type.WithDisplayName("Game Tip")

                .Creatable()
                .Listable()
                .Draftable()
                .Versionable()
                .Securable()

                .WithPart("TitlePart")

                .WithPart(nameof(GameTipPart))
        );

        return 1;
    }


    public async Task<int> UpdateFrom1Async()
    {
        await _contentDefinitionManager.AlterTypeDefinitionAsync(
            "GameTipWidget",
            type => type
                .WithDisplayName("Game Tip Widget")
                .Stereotype("Widget")
                .WithPart(nameof(GameTipPart))
        );

        return 2;
    }

}

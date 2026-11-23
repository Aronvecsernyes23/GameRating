
using DojoCourse.Module.Indexes;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace DojoCourse.Module.Migrations;

public class GameDetailsMigration : DataMigration
{
    public async Task<int> CreateAsync()
    {
        // Saját YesSQL MapIndex tábla létrehozása.
        await SchemaBuilder.CreateMapIndexTableAsync<GameDetailsIndex>(
            table => table
                .Column<string>(
                    nameof(GameDetailsIndex.Developer),
                    column => column.WithLength(255))

                .Column<string>(
                    nameof(GameDetailsIndex.Publisher),
                    column => column.WithLength(255))
        );

        // SQL-index a gyorsabb Developer szűréshez.
        await SchemaBuilder.AlterIndexTableAsync<GameDetailsIndex>(
            table => table.CreateIndex(
                "IDX_GameDetailsIndex_Developer",
                nameof(GameDetailsIndex.Developer))
        );

        return 1;
    }
}

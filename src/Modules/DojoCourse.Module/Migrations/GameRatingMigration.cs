
using DojoCourse.Module.Indexes;
using OrchardCore.Data.Migration;
using YesSql.Sql;

namespace DojoCourse.Module.Migrations;

public class GameRatingMigration : DataMigration
{
    public async Task<int> CreateAsync()
    {
        await SchemaBuilder
            .CreateMapIndexTableAsync<GameRatingIndex>(
                table => table
                    .Column<string>(
                        nameof(GameRatingIndex.GameContentItemId),
                        column => column.WithLength(64))

                    .Column<string>(
                        nameof(GameRatingIndex.UserId),
                        column => column.WithLength(128))

                    .Column<int>(
                        nameof(GameRatingIndex.Score))
            );

        // Gyors lekérdezés játék és felhasználó alapján.
        await SchemaBuilder
            .AlterIndexTableAsync<GameRatingIndex>(
                table => table.CreateIndex(
                    "IDX_GameRating_Game_User",
                    nameof(GameRatingIndex.GameContentItemId),
                    nameof(GameRatingIndex.UserId))
            );

        return 1;
    }
}

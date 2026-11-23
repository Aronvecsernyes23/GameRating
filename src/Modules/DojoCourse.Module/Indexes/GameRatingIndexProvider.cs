
using DojoCourse.Module.Models;
using YesSql.Indexes;

namespace DojoCourse.Module.Indexes;

public class GameRatingIndexProvider
    : IndexProvider<GameRatingRecord>
{
    public override void Describe(
        DescribeContext<GameRatingRecord> context)
    {
        context.For<GameRatingIndex>()
            .Map(rating => new GameRatingIndex
            {
                GameContentItemId =
                    rating.GameContentItemId,

                UserId = rating.UserId,

                Score = rating.Score
            });
    }
}

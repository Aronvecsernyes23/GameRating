
using YesSql.Indexes;

namespace DojoCourse.Module.Indexes;

public class GameRatingIndex : MapIndex
{
    public string GameContentItemId { get; set; } =
        string.Empty;

    public string UserId { get; set; } =
        string.Empty;

    public int Score { get; set; }
}

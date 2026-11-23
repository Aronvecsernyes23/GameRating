
using YesSql.Indexes;

namespace DojoCourse.Module.Indexes;

public class GameDetailsIndex : MapIndex
{
    public string Developer { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;
}


using System.Text.Json.Nodes;
using OrchardCore.ContentManagement;
using YesSql.Indexes;

namespace DojoCourse.Module.Indexes;

public class GameDetailsIndexProvider
    : IndexProvider<ContentItem>
{
    public override void Describe(
        DescribeContext<ContentItem> context)
    {
        context.For<GameDetailsIndex>()
            .When(item => item.ContentType == "Game")
            .Map(item => new GameDetailsIndex
            {
                Developer = item.Content["Game"]?
                    ["Developer"]?["Text"]?.ToString()
                    ?? string.Empty,

                Publisher = item.Content["Game"]?
                    ["Publisher"]?["Text"]?.ToString()
                    ?? string.Empty
            });
    }
}

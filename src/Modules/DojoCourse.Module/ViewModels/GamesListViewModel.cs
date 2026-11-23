
namespace DojoCourse.Module.ViewModels;

public class GameListItemViewModel
{
    public string ContentItemId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Developer { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string? CoverUrl { get; set; }

    public string GameUrl { get; set; } = string.Empty;
}

public class GamesListViewModel
{
    public string? DeveloperFilter { get; set; }

    public DateTime GeneratedUtc { get; set; }

    public List<GameListItemViewModel> Games { get; set; } = new();
}


namespace DojoCourse.Module.ViewModels;

public class GameSuggestionRowViewModel
{
    public string ContentItemId { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public bool Published { get; set; }

    public string Status { get; set; } = "Legacy";
}

public class GameModerationViewModel
{
    public List<GameSuggestionRowViewModel> Suggestions { get; set; }
        = new();
}

public class GameSuggestionAuthorizationViewModel
{
    public string Title { get; set; } = string.Empty;

    public bool IsExisting { get; set; }

    public bool CanEdit { get; set; }
}

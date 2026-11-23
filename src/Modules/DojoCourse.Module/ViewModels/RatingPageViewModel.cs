
namespace DojoCourse.Module.ViewModels;

public class RatingPageViewModel
{
    public string GameContentItemId { get; set; } =
        string.Empty;

    public string GameTitle { get; set; } =
        string.Empty;

    public string GameUrl { get; set; } =
        string.Empty;

    public int RatingCount { get; set; }

    public double? AverageRating { get; set; }

    public int? YourScore { get; set; }

    public bool IsAuthenticated { get; set; }

    public bool Saved { get; set; }
}


namespace DojoCourse.Module.Models;

public class GameSubmissionData
{
    public string Title { get; set; } = string.Empty;

    public DateOnly? ReleaseDate { get; set; }

    public string Developer { get; set; } = string.Empty;

    public string Publisher { get; set; } = string.Empty;

    public string Platforms { get; set; } = string.Empty;

    public string Genre { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CoverPath { get; set; } = string.Empty;

    // Pending, Approved vagy Rejected
    public string Status { get; set; } = "Pending";

    // Az admin elutasítási indoklása
    public string ReviewReason { get; set; } = string.Empty;

    // Elfogadás után létrehozott Game azonosítója
    public string ApprovedGameId { get; set; } = string.Empty;

    // A beküldő felhasználó Orchard-azonosítója
    public string SubmittedByUserId { get; set; } = string.Empty;
}

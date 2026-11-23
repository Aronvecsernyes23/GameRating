
namespace DojoCourse.Module.Models;

public class GameRatingRecord
{
    // YesSQL dokumentumazonosító
    public int Id { get; set; }

    // Melyik játékot értékelték?
    public string GameContentItemId { get; set; } =
        string.Empty;

    // Melyik bejelentkezett felhasználó?
    public string UserId { get; set; } =
        string.Empty;

    // Pontszám 1 és 10 között
    public int Score { get; set; }

    // Utolsó értékelés vagy módosítás ideje
    public DateTime UpdatedUtc { get; set; }
}


using System.ComponentModel.DataAnnotations;

namespace DojoCourse.Module.ViewModels;

public class GameTipPartViewModel
{
    [Required(ErrorMessage = "Game name is required.")]
    [StringLength(200)]
    public string GameName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tip text is required.")]
    [StringLength(2000)]
    public string TipText { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a difficulty.")]
    public string Difficulty { get; set; } = string.Empty;
    public string PreviewText { get; set; } = string.Empty;
}

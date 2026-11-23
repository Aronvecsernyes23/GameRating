
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace DojoCourse.Module.ViewModels;

public class GameSubmissionFormViewModel
{
    [Required]
    [StringLength(180)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? ReleaseDate { get; set; }

    [Required]
    [StringLength(150)]
    public string Developer { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Publisher { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Platforms { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string Genre { get; set; } = string.Empty;

    [Required]
    [StringLength(3000)]
    public string Description { get; set; } = string.Empty;

    // Kezdetben opcionális.
    // Az admin később pótolhatja vagy lecserélheti.
    public IFormFile? CoverImage { get; set; }
}

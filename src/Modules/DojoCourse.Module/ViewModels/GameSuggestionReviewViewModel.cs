
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using DojoCourse.Module.Models;

namespace DojoCourse.Module.ViewModels;

public class GameSuggestionDetailsViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    public GameSubmissionData? Data { get; set; }

    public string? CoverUrl { get; set; }

    public string? GameUrl { get; set; }
}

public class EditGameSuggestionViewModel
{
    [Required]
    public string Id { get; set; } = string.Empty;

    [Required, StringLength(180)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateOnly? ReleaseDate { get; set; }

    [Required, StringLength(150)]
    public string Developer { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Publisher { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string Platforms { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Genre { get; set; } = string.Empty;

    [Required, StringLength(3000)]
    public string Description { get; set; } = string.Empty;

    // Opcionális borítócsere
    public IFormFile? CoverImage { get; set; }

    public string? CurrentCoverUrl { get; set; }
}

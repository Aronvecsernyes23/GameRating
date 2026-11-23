
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace DojoCourse.Module.Models;

public class GameTipPart : ContentPart
{
    public TextField GameName { get; set; } = new();

    public TextField TipText { get; set; } = new();

    public TextField Difficulty { get; set; } = new();

    public string PreviewText { get; set; } = string.Empty;
}

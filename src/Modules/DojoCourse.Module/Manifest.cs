using OrchardCore.Modules.Manifest;

[assembly: Module(
    Name = "DojoCourse.Module",
    Author = "The Orchard Core Team",
    Website = "https://orchardcore.net",
    Version = "0.0.1",
    Description = "Custom game database functionality.",
    Dependencies = new[]
    {
        "OrchardCore.Media",
        "OrchardCore.ContentFields",
        "OrchardCore.Title",
        "OrchardCore.Users"
    }
)]

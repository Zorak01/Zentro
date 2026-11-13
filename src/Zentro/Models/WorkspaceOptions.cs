namespace Zentro.Models;

public static class WorkspaceOptions
{
    public static IReadOnlyList<string> Statuses { get; } =
        Array.AsReadOnly(new[] { "To do", "Doing", "Done" });

    public static IReadOnlyList<string> Priorities { get; } =
        Array.AsReadOnly(new[] { "Low", "Normal", "High" });
}

namespace Zentro.Models;

public sealed record WorkspaceItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Title { get; init; } = "";
    public string Body { get; init; } = "";
    public string Status { get; init; } = "To do";
    public string Priority { get; init; } = "Normal";
    public bool Note { get; init; }
    public DateTime Updated { get; init; } = DateTime.Now;

    public string Eyebrow => Note ? "NOTE" : Priority.ToUpperInvariant();
    public string Footer => Note ? $"Edited {Updated:MMM d}" : Status == "Done" ? "✓  Completed" : Status;
    public string Preview => Body.Length > 180 ? Body[..180] + "…" : Body;
}

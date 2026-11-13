using System.ComponentModel;
using System.IO;
using Zentro.Models;
using Zentro.Services;

namespace Zentro.ViewModels;

public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly WorkspaceStore store;
    private List<WorkspaceItem> items = [];
    private List<WorkspaceItem>? undoSnapshot;
    private string query = "";
    private bool notes;
    public event PropertyChangedEventHandler? PropertyChanged;
    public bool CanWrite { get; private set; } = true;
    public string SaveStatus { get; private set; } = "Saved locally";
    public string Notice { get; private set; } = "";
    public bool HasNotice => Notice.Length > 0;
    public bool CanUndo => undoSnapshot != null;
    public string DateLabel => DateTime.Now.ToString("dddd, MMMM d");
    public string Query { get => query; set { query = value; Refresh(); } }
    public bool IsNotes { get => notes; set { notes = value; Refresh(); } }
    public string Heading => notes ? "Notes" : "Task board";
    public string Subtitle => notes ? "Write and search notes." : "Track tasks from To do to Done.";
    public string AddLabel => notes ? "+  New note" : "+  New task";
    public string SearchHint => notes ? "Search notes…" : "Search tasks…";
    public string Summary
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                string kind = notes ? "notes" : "tasks";
                return $"{Filtered.Count()} matching {kind}";
            }

            if (notes)
            {
                return $"{Filtered.Count()} notes";
            }

            int total = items.Count(i => !i.Note);
            int completed = items.Count(i => !i.Note && i.Status == "Done");
            return $"{completed} completed / {total} tasks";
        }
    }
    public IEnumerable<WorkspaceItem> Filtered => items.Where(i => i.Note == notes && (i.Title + " " + i.Body).Contains(query.Trim(), StringComparison.OrdinalIgnoreCase)).OrderByDescending(i => i.Updated);
    public WorkspaceItem[] Todo => Filtered.Where(i => i.Status == "To do").ToArray();
    public WorkspaceItem[] Doing => Filtered.Where(i => i.Status == "Doing").ToArray();
    public WorkspaceItem[] Done => Filtered.Where(i => i.Status == "Done").ToArray();
    public WorkspaceItem[] Notes => Filtered.ToArray();
    public bool Empty => !Filtered.Any();
    public string EmptyTitle => query.Length > 0 ? "No matches found" : notes ? "No notes" : "No tasks";
    public string EmptyBody => query.Length > 0 ? "Try another keyword or clear the search." : notes ? "Create a note." : "Create a task.";

    public MainViewModel(WorkspaceStore? store = null)
    {
        this.store = store ?? new WorkspaceStore();
        try { items = this.store.Load(); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Text.Json.JsonException)
        {
            CanWrite = false;
            SaveStatus = "Workspace unavailable";
            Notice = "Could not load your workspace. Your file was left untouched. " + ex.Message;
        }
    }

    public string? Commit(WorkspaceItem item)
    {
        if (string.IsNullOrWhiteSpace(item.Title)) return "Enter a title.";
        var next = items.Where(i => i.Id != item.Id).Append(item).ToList();
        return Apply(next, "Saved.");
    }

    public string? Move(string id, string status)
    {
        var item = items.FirstOrDefault(i => i.Id == id && !i.Note);
        if (item == null || item.Status == status) return null;
        return Commit(item with { Status = status, Updated = DateTime.Now });
    }

    public string? Delete(WorkspaceItem item) => Apply(items.Where(i => i.Id != item.Id).ToList(), $"{(item.Note ? "Note" : "Task")} deleted.");

    public void Undo()
    {
        if (undoSnapshot == null) return;
        var previous = undoSnapshot;
        if (Apply(previous, "Change undone.") == null) { undoSnapshot = null; Refresh(); }
    }

    private string? Apply(List<WorkspaceItem> next, string message)
    {
        if (!CanWrite) return "Your workspace could not be loaded. Restore the file and restart Zentro.";
        try { store.Save(next); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Notice = "Could not save. " + ex.Message;
            SaveStatus = "Changes were not saved";
            Refresh();
            return Notice;
        }
        undoSnapshot = items;
        items = next;
        SaveStatus = "Saved locally";
        Notice = message;
        Refresh();
        return null;
    }

    private void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
}

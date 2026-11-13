using System.IO;
using System.Text.Json;
using Zentro.Models;

namespace Zentro.Services;

/// <summary>All disk access lives here. Writes replace the file only after serialization succeeds.</summary>
public sealed class WorkspaceStore
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, IgnoreReadOnlyProperties = true };
    public string FilePath { get; }
    private readonly string? legacyPath;

    public WorkspaceStore(string? filePath = null, string? legacyFilePath = null)
    {
        string appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        FilePath = filePath ?? Path.Combine(appData, "Zentro", "workspace.json");
        legacyPath = legacyFilePath ?? (filePath == null ? Path.Combine(appData, "organiZe", "workspace.json") : null);
    }

    public List<WorkspaceItem> Load()
    {
        if (File.Exists(FilePath)) return Read(FilePath);
        if (legacyPath != null && File.Exists(legacyPath))
        {
            var imported = Read(legacyPath);
            Save(imported); // The old workspace remains untouched.
            return imported;
        }
        return [];
    }

    private static List<WorkspaceItem> Read(string path)
    {
        var items = JsonSerializer.Deserialize<List<WorkspaceItem>>(File.ReadAllText(path))
            ?? throw new InvalidDataException("The workspace file is empty.");
        var ids = new HashSet<string>();
        foreach (var item in items)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id) || !ids.Add(item.Id)
                || string.IsNullOrWhiteSpace(item.Title) || item.Body == null
                || !new[] { "To do", "Doing", "Done" }.Contains(item.Status)
                || !new[] { "Low", "Normal", "High" }.Contains(item.Priority))
                throw new InvalidDataException("The workspace contains an invalid item. Your file has been left untouched.");
        }
        return items;
    }

    public void Save(IReadOnlyList<WorkspaceItem> items)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(FilePath))!);
        string temporary = FilePath + ".tmp";
        try
        {
            File.WriteAllText(temporary, JsonSerializer.Serialize(items, JsonOptions));
            if (File.Exists(FilePath)) File.Replace(temporary, FilePath, FilePath + ".bak");
            else File.Move(temporary, FilePath);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }
}

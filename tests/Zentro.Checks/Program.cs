using Zentro.Models;
using Zentro.Services;
using Zentro.ViewModels;

string directory = Path.Combine(Path.GetTempPath(), "Zentro.Checks", Guid.NewGuid().ToString("N"));
Directory.CreateDirectory(directory);
int passed = 0;
void Check(bool value, string name)
{
    if (!value) throw new Exception("FAILED: " + name);
    Console.WriteLine("PASS " + name);
    passed++;
}
try
{
    var path = Path.Combine(directory, "workspace.json");
    var store = new WorkspaceStore(path);
    Check(store.Load().Count == 0, "new workspace starts empty");
    var model = new MainViewModel(store);
    var task = new WorkspaceItem { Title = "Plan release", Body = "Polish search", Priority = "High" };
    Check(model.Commit(task) == null && store.Load().Single().Title == task.Title, "saved task round-trips");
    model.Move(task.Id, "Doing");
    Check(model.Doing.Length == 1 && store.Load()[0].Status == "Doing", "move persists status");
    Check(File.Exists(path + ".bak"), "previous version is backed up");
    model.Query = "POLISH";
    Check(model.Doing.Length == 1, "search matches details case-insensitively");
    model.Query = "missing";
    Check(model.Empty, "search empty state");
    model.Query = "";
    model.Delete(task);
    Check(store.Load().Count == 0, "delete persists");
    model.Undo();
    Check(store.Load().Single().Status == "Doing", "undo restores deleted task");
    model.Commit(new WorkspaceItem { Title = "An idea", Note = true });
    model.IsNotes = true;
    Check(model.Notes.Length == 1 && model.Notes[0].Note, "notes separated from task board");
    var legacy = Path.Combine(directory, "legacy.json");
    File.WriteAllText(legacy, "[{\"Id\":\"old\",\"Title\":\"Existing task\",\"Body\":\"\",\"Status\":\"Done\",\"Priority\":\"Normal\",\"Note\":false}]");
    var migrated = new WorkspaceStore(Path.Combine(directory, "migrated.json"), legacy);
    Check(migrated.Load().Single().Title == "Existing task" && File.Exists(legacy), "legacy import preserves original");
    File.WriteAllText(path, "broken json");
    var broken = new MainViewModel(store);
    Check(!broken.CanWrite && broken.Commit(task) != null && File.ReadAllText(path) == "broken json", "corrupt workspace stays untouched");
    string blocked = Path.Combine(directory, "blocked");
    var failingStore = new WorkspaceStore(Path.Combine(blocked, "workspace.json"));
    var failingModel = new MainViewModel(failingStore);
    File.WriteAllText(blocked, "file instead of directory");
    Check(failingModel.Commit(task) != null && failingModel.Todo.Length == 0, "failed save does not mutate UI data");
    Console.WriteLine($"{passed} checks passed.");
}
finally { Directory.Delete(directory, true); }

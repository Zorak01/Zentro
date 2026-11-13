# Zentro

A calm Windows workspace for tasks and notes. Move tasks through **To do → Doing → Done**, capture ideas, and find them quickly. Dark surfaces, purple-blue accents, and an original flat purple-blue mark.

![Zentro task board](docs/preview.png)

## Use

Run `Zentro.exe` from the Windows x64 release. No separate .NET installation, account, or network connection required. Keep one copy open at a time.

- Click a card or focus it and press **Enter** to edit.
- Drag tasks between columns, or change status in the editor.
- Choose **My notes** to capture ideas.
- **Ctrl+N** creates; **Ctrl+F** searches; **Ctrl+S** saves in the editor; **Esc** cancels.
- **Undo** reverses your last saved action, including deletion. **Ctrl+Z** does the same when you are outside a text field.

Only explicit saves persist editor changes. Search is local and includes titles and details. Empty workspaces start clean, without sample tasks.

## Build

Windows and the .NET 9 SDK are required for development. There are no third-party runtime dependencies.

```powershell
./build.ps1          # run workspace checks and build
./build.ps1 -Publish # create artifacts/Zentro/Zentro.exe
```

GitHub Actions runs the same checks and builds a downloadable Windows executable on each push.

## Layout

```text
src/Zentro/
  Assets/        Final PNG and multi-size Windows icon
  Models/        Workspace record
  Services/      JSON persistence and legacy import
  ViewModels/    Filtering, updates, undo, save state
  Views/         Board, reusable column, and editor
  Styles/        Shared visual theme
tests/Zentro.Checks/   Dependency-free workspace checks
```

`bin`, `obj`, and `artifacts` are ignored. Generated alternatives, credentials, user data, and binaries do not belong in Git.

## Your data

Everything stays in `%LOCALAPPDATA%/Zentro/workspace.json`. The last saved version is kept at `workspace.json.bak`. Writes replace the file atomically; failed saves leave the visible workspace unchanged. Invalid files are preserved and editing is disabled until you restore a valid workspace and restart.

On first launch, Zentro imports the earlier organiZe workspace if present, leaving the old file untouched. Close the old app before migrating, and use Zentro going forward. Undo is in memory and resets on restart. Back up the data folder to move your workspace to another computer.

## Branding

The included purple-blue mark was generated for Zentro using OpenAI image generation. It uses a simple stepped motif, distinct from the Zorak website logo.

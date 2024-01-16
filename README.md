# Non-Destructive MMD

An experimental package for VRChat avatars to add MMD blendshapes without affecting avatar files.

Requires Modular Avatar and NDMF to be installed.

This package is currently subject to breaking changes.

## Development

### Style

This project uses `dotnet format` with `.editorconfig` to enforce style guidelines.

TODO: Add GitHub Action to automate this.

If csproj files are not generated:

```powershell
& "C:\Program Files\Unity\Hub\Editor\2019.4.31f1\Editor\Unity.exe" -batchmode -nographics -logFile - -projectPath . -executeMethod Packages.Rider.Editor.RiderScriptEditor.SyncSolution -quit
```

Then run `dotnet format`:

```powershell
dotnet format .\enitimeago.non-destructive-mmd.editor.csproj
dotnet format .\enitimeago.non-destructive-mmd.runtime.csproj
```

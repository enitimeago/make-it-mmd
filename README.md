# Make It MMD

Make It MMD is a **non-destructive** tool (using NDMF) for adding MMD world compatibility to your VRChat avatars.

It's currently experimental, so please be aware it may not function as expected, and may undergo breaking changes.

Make It MMDは、NDMFを利用して、**非破壊な**VRChatアバターをMMDワールド対応するツールです。

本ツールは実験的なものであり、期待どおりに動かない可能性があり、動作が変更される可能性もあります。予めご了承下さい。

## Current Status

This tool has only been minimally tested with Sue v1.3.6 by mio3io at time of writing.

現時点は、mio3ioさんの「透羽」v1.3.6のみで動作することをわずかに確認されておりました。

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

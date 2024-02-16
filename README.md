# Make It MMD

![GitHub release (latest by date)](https://img.shields.io/github/v/release/enitimeago/make-it-mmd?label=release)

Make It MMD makes it easy to **create** and **share** MMD blend shapes for VRChat avatars, based on their existing facial expressions.

- 🌟 It's **non-destructive**, running only when you build your avatar.
- ✏️ Settings are **editable any time**, no need to revert to the original mesh and try again.
- 💌 Plus **easy .unitypackage export** of your settings, no need to worry about sharing your avatar's mesh data.

Make It MMDは、VRChatアバターの既存の表情を基づいて、MMDブレンドシェープを**作成すること**および**設定を共有すること**を容易にします。

- 🌟 本ツールは**非破壊的**で、アバターをビルドする際にのみ実行されます。
- ✏️ 設定はいつでも**編集可能**で、元のメッシュに戻って再試行する必要はありません。
- 💌 さらに、設定を**簡単に.unitypackageとしてエクスポート**できるので、アバターのメッシュデータを共有する恐れがありません。

## Who is this for?

If your avatar does not have MMD blendshapes. (Maybe also in the future: If you want to modify your existing MMD blendshapes too.) If you avatar already has MMD blendshapes and it does not work in MMD worlds, you will need to debug your avatar's animators, which unforunately is not simple to automate!

## Current Status

This tool has only been minimally tested with Unity 2019 and Sue v1.3.6 by mio3io at time of writing.

It's currently experimental, so please be aware it may not function as expected, and may undergo breaking changes.

現時点は、Unity 2019で、mio3ioさんの「透羽」v1.3.6のみで動作することをわずかに確認されておりました。

このツールは実験的なものであり、期待どおりに動かない可能性があり、動作が変更される可能性もあります。予めご了承下さい。

## Installation

- 📦 Install using VRChat Creator Companion: [Add VPM repository](https://enitimeago.github.io/vpm-repos/)
- 📦 Install using .unitypackage: [See Releases](https://github.com/enitimeago/make-it-mmd/releases)

## Usage

以下は日本語の場合はGoogle TranslateまたはDeepLなどでお願いします。

Please be aware this tool will not work if the face has any name other than "Body". It will force all FX animators to Write Defaults ON. This may cause unexpected behavior. (Usage with FaceEmo is recommended.)

- Right-click your avatar in the hierarchy and select "Create Empty", and name your new object "MMD"
  - **Advanced users:** This is just the recommended approach. You can use any object underneath your avatar, or even your avatar itself.
- Click on "Add Component" in your new object's inspector
- Search for "Make It MMD", or select "Scripts/Make It MMD"
- Click Open Editor
- On the left, you will see a list of MMD blend shapes. Select a blend shape to continue
- On the right, you will see the list of your avatar's blend shapes. Select a blend shape that corresponds to the MMD blend shape.
- Repeat for all MMD blend shapes you wish to support. The most important are generally あ　い　う　え　お.
- Click the Play button in the toolbar. Check that your avatar builds.
- Click on "Body" on your hierarchy, and check the blend shapes in the inspector. You should see the MMD blend shapes listed at the bottom.
- Adjust the MMD blend shapes to confirm that they function correctly.
- Exit the Play mode
- Build and publish your avatar!

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

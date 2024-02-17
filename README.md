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

Please be aware this tool currently will not work if the face has any name other than "Body".

現時点は、Unity 2019で、mio3ioさんの「透羽」v1.3.6のみで動作することをわずかに確認されておりました。

このツールは実験的なものであり、期待どおりに動かない可能性があり、動作が変更される可能性もあります。予めご了承下さい。

アバターの顔のメッシュが「Body」で呼ばれていない場合は、このツールは現在機能しません。

## Installation

- 📦 Install using VRChat Creator Companion: [Add VPM repository](https://enitimeago.github.io/vpm-repos/)
- 📦 Install using .unitypackage: [See Releases](https://github.com/enitimeago/make-it-mmd/releases)

## Usage

現在、以下の使用方法は英語のみです。日本語の場合はGoogle TranslateまたはDeepLなどでお願いします。

In order to support facial animations in MMD worlds, your avatar must meet the following conditions:

1. Your avatar's face mesh must have blend shapes named the same as MMD morphs.
2. Your avatar's FX Animator States must *all* have Write Defaults enabled.

Make It MMD provides a Unity component to assist with each of these.

### Create blend shapes

The "Make MMD BlendShapes" component generates blend shapes for MMD morphs, with a visual editor to preview which blend shapes you can select for each MMD morph. It's non-destructive, so it doesn't make a permanent copy of your avatar's face mesh. You can change your choices at any time.

- In your avatar or any object inside your avatar, click on "Add Component" in the object inspector
- Search for "Make MMD BlendShapes", or select "Make It MMD/Make MMD Blendshapes"
- Click Open Editor
- On the left, you will see a list of MMD morphs. Select a blend shape to continue
- On the right, you will see the list of your avatar's blend shapes. Select one or more blend shapes that correspond to the MMD blend shape.
- Repeat for all MMD morphs you wish to support.
- You should see the new MMD blend shapes applied to your avatar's face mesh when you enter Play mode.

### Set Write Defaults

The "Avatar Write Defaults" component can help modify all FX Animator States on your avatar to set Write Defaults ON. It's non-destructive, so it only modifies your avatar's FX when the component is enabled in your avatar.

**WARNING:** Forcing all Animator States to Write Defaults ON may cause unexpected behavior with other gimmicks installed in your avatar!

You don't need this component if your avatar already uses Write Defaults ON for all Animator States, or if you're using other tools that achieve this for you.

- In your avatar or any object inside your avatar, click on "Add Component" in the object inspector
- Search for "Avatar Write Defaults", or select "Make It MMD/Avatar Write Defaults"
- Click the checkbox to force all Write Defaults ON when you enter Play mode or build your avatar.

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

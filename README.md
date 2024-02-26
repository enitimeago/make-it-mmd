# Make It MMD

![GitHub release (latest by date)](https://img.shields.io/github/v/release/enitimeago/make-it-mmd?label=release)

Make It MMD is a set of **non-destructive** Unity extensions to help VRChat avatars support MMD worlds.

- 🌟 **"Make MMD BlendShapes"** helps generate MMD blend shapes based on your avatar's existing facial expressions, without permanently modifying the orignal mesh. It comes with a visual editor that lets you edit expressions for each MMD morph at any time.
- ✏️ **"Avatar Write Defaults"** and **"Remove FX Animator Layers"** helps modify your avatar's FX animator controller to improve MMD compatibility, without permanently modifying the original animator controller.
- 💌 Plus **MMD Scan and Import** lets you import from self-made blend shapes or compatible destructive MMD tools, and **Export .unitypackage** helps you easily share blend shape settings, without the risk of sharing your avatar's 3D mesh.

## Old intro, TODO remove

Make It MMD makes it easy to **create** and **share** MMD blend shapes for VRChat avatars, based on their existing facial expressions.

- 🌟 It's **non-destructive**, running only when you build your avatar.
- ✏️ Settings are **editable any time**, no need to revert to the original mesh and try again.
- 💌 Plus **easy .unitypackage export** of your settings, no need to worry about sharing your avatar's mesh data.

Make It MMDは、VRChatアバターの既存の表情を基づいて、MMDブレンドシェープを**作成すること**および**設定を共有すること**を容易にします。

- 🌟 本ツールは**非破壊的**で、アバターをビルドする際にのみ実行されます。
- ✏️ 設定はいつでも**編集可能**で、元のメッシュに戻って再試行する必要はありません。
- 💌 さらに、設定を**簡単に.unitypackageとしてエクスポート**できるので、アバターのメッシュデータを共有する恐れがありません。

## Known Issues

Please be aware this tool currently will not work if the face has any name other than "Body".

Avatars with existing MMD blend shapes are unsupported. Support is planned for a future release.

アバターの顔のメッシュが「Body」で呼ばれていない場合は、このツールは現在機能しません。

既存のMMDブレンドシェイプを持つアバターはサポートされていません。サポートするのは予定されています。

## Installation

- 📦 Install using VRChat Creator Companion: [Add VPM repository](https://enitimeago.github.io/vpm-repos/)
- 📦 Install using .unitypackage: [See Releases](https://github.com/enitimeago/make-it-mmd/releases)

## Usage

現在、以下の使用方法は英語のみです。日本語の場合はGoogle TranslateまたはDeepLなどでお願いします。

In order to support facial animations in popular VRChat MMD worlds, your avatar must meet the following conditions:

1. Your avatar's face mesh must have blend shapes named the same as MMD morphs.
2. Your avatar's FX Animator States must *all* have Write Defaults enabled.
3. Your avatar's first three FX Animator Layers should include the "Left Hand" and "Right Hand" layers.

Make It MMD provides a Unity component to assist with each of these.

### Make MMD BlendShapes

The "Make MMD BlendShapes" component generates blend shapes for MMD morphs, with a visual editor to preview which blend shapes you can select for each MMD morph. It's non-destructive, so it doesn't make a permanent copy of your avatar's face mesh. You can change your choices at any time.

- In your avatar or any object inside your avatar, click on "Add Component" in the object inspector
- Search for "Make MMD BlendShapes", or select "Make It MMD/Make MMD Blendshapes"
- Click Open Editor
- On the left, you will see a list of MMD morphs. Select a blend shape to continue
- On the right, you will see the list of your avatar's blend shapes. Select one or more blend shapes that correspond to the MMD blend shape.
- Repeat for all MMD morphs you wish to support.
- You should see the new MMD blend shapes applied to your avatar's face mesh when you enter Play mode.

### Avatar Write Defaults

The "Avatar Write Defaults" component can help modify all FX Animator States on your avatar to set Write Defaults ON. It's non-destructive, so it only modifies your avatar's FX when the component is enabled in your avatar.

**WARNING:** Forcing all Animator States to Write Defaults ON may cause unexpected behavior with other gimmicks installed in your avatar!

You don't need this component if your avatar already uses Write Defaults ON for all Animator States, or if you're using other tools that achieve this for you.

- In your avatar or any object inside your avatar, click on "Add Component" in the object inspector
- Search for "Avatar Write Defaults", or select "Make It MMD/Avatar Write Defaults"
- Click the checkbox to force all Write Defaults ON when you enter Play mode or build your avatar.

### Remove FX Animator Layers

TODO document this

## Acknowledgements

Make It MMD bundles and redistributes code from third-party software. The licenses for these software may be found in the headers of their respective source files. The author would like to acknowledge the developers of these third-party software:

- BlendshapeCombiner by Chigiri Tsutsumi https://github.com/chigirits/BlendShapeCombiner
- Blendshape Viewer by Haï https://github.com/hai-vr/blendshape-viewer-vcc
- d4rkAvatarOptimizer by d4rkpl4y3r https://github.com/d4rkc0d3r/d4rkAvatarOptimizer
- Modular Avatar by bd_ https://github.com/bdunderscore/modular-avatar

## Development

### Style

This project uses `dotnet format` with `.editorconfig` to enforce style guidelines.

These lints will run automatically when creating pull requests.

You can also run these manually:

```powershell
dotnet format .\enitimeago.non-destructive-mmd.editor.csproj
dotnet format .\enitimeago.non-destructive-mmd.runtime.csproj
dotnet format .\enitimeago.non-destructive-mmd.tests.csproj
```

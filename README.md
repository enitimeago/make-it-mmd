# Make It MMD

![GitHub release (latest by date)](https://img.shields.io/github/v/release/enitimeago/make-it-mmd?label=release)

Make It MMD is a set of **non-destructive** Unity extensions to help VRChat avatars support MMD worlds.

## Installation

- 📦 Install using VRChat Creator Companion: [Add VPM repository](https://enitimeago.github.io/vpm-repos/)
- 📦 Install using .unitypackage: [See Releases](https://github.com/enitimeago/make-it-mmd/releases)

## Usage

You can find usage on the [website](https://enitimeago.github.io/make-it-mmd/).
The docs are also located in the `docs/` folder of this repo.

## Acknowledgements

Make It MMD bundles and redistributes code from third-party software. The licenses for these software may be found in the headers of their respective source files. The author would like to acknowledge the developers of these third-party software:

- Blendshape Viewer by Haï https://github.com/hai-vr/blendshape-viewer-vcc
- d4rkAvatarOptimizer by d4rkpl4y3r https://github.com/d4rkc0d3r/d4rkAvatarOptimizer
- Modular Avatar by bd_ https://github.com/bdunderscore/modular-avatar

## Development

### Release management

All development occurs on the `main` branch:

- New releases are cut from the `main` branch as appropriate, so long as it is green.
- There is no `develop` branch.
- Therefore, `main` will have changes that may not be ready for broader release.
- Older releases are supported by cherry-picking to `support/x.x` branches, a practice inspired by GitFlow.

### Setup

On macOS, you will need `vrc-get` which you can install with `brew install vrc-get`.

Then run:

```shell
vrc-get resolve --project .
```

### Code style

This project uses `dotnet format` with `.editorconfig` to enforce style guidelines.

These lints will run automatically when creating pull requests.

You can also run these manually:

```powershell
dotnet format .\enitimeago.non-destructive-mmd.editor.csproj
dotnet format .\enitimeago.non-destructive-mmd.runtime.csproj
dotnet format .\enitimeago.non-destructive-mmd.tests.csproj
```

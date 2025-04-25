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

#### Windows

Run prepare.py from the repo root.

#### macOS

Run prepare.py from the repo root.

You will need `vrc-get` which you can install with `brew install vrc-get`.

Then run:

```shell
vrc-get resolve --project .
```

### Dependency management

Copybara is used to clone some external dependencies into this repo directly.
Usage is motivated by two reasons:

1. Applying significant patches reproducibly
2. Avoiding submodules so this repo can work independent from upstream

If upstream hasn't broken the patches then you don't have to touch Copybara,
the GitHub Action `.github/update-third-party.yml` abstracts updates.

Some notes in case you do have to touch it:

- Downloading the [pre-compiled .jar](https://github.com/google/copybara/releases) is much easier than compiling from scratch
- On Windows you can work around the missing `$HOME` error by specifying `--output-root`
  - e.g. `java -jar copybara_deploy.jar --output-root C:\Users\[USERNAME]\copybara\out help`
- On Windows PowerShell if you use `--dry-run` then the correct way to check is `$env:GIT_DIR='foo'; git log`

### Code style

This project uses `dotnet format` with `.editorconfig` to enforce style guidelines.

These lints will run automatically when creating pull requests.

You can also run these manually:

```powershell
dotnet format .\enitimeago.non-destructive-mmd.editor.csproj
dotnet format .\enitimeago.non-destructive-mmd.runtime.csproj
dotnet format .\enitimeago.non-destructive-mmd.tests.csproj
```

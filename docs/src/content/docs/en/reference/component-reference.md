---
title: Component Reference
---

In order to support facial animations in popular VRChat MMD worlds, your avatar must meet the following conditions:

1. Your avatar's face mesh must have blend shapes named the same as MMD morphs.
2. Your avatar's FX Animator States must *all* have Write Defaults enabled.
3. Your avatar's first three FX Animator Layers should include the "Left Hand" and "Right Hand" layers.

Make It MMD provides a Unity component to assist with each of these.

## Make MMD BlendShapes

The "Make MMD BlendShapes" component generates blend shapes for MMD morphs, with a visual editor to preview which blend shapes you can select for each MMD morph. It's non-destructive, so it doesn't make a permanent copy of your avatar's face mesh. You can change your choices at any time.

- In your avatar or any object inside your avatar, click on "Add Component" in the object inspector
- Search for "Make MMD BlendShapes", or select "Make It MMD/Make MMD Blendshapes"
- Click Open Editor
- On the left, you will see a list of MMD morphs. Select a blend shape to continue
- On the right, you will see the list of your avatar's blend shapes. Select one or more blend shapes that correspond to the MMD blend shape.
- Repeat for all MMD morphs you wish to support.
- You should see the new MMD blend shapes applied to your avatar's face mesh when you enter Play mode.

## Avatar Write Defaults

The "Avatar Write Defaults" component can help modify all FX Animator States on your avatar to set Write Defaults ON. It's non-destructive, so it only modifies your avatar's FX when the component is enabled in your avatar.

**WARNING:** Forcing all Animator States to Write Defaults ON may cause unexpected behavior with other gimmicks installed in your avatar!

You don't need this component if your avatar already uses Write Defaults ON for all Animator States, or if you're using other tools that achieve this for you.

- In your avatar or any object inside your avatar, click on "Add Component" in the object inspector
- Search for "Avatar Write Defaults", or select "Make It MMD/Avatar Write Defaults"
- Click the checkbox to force all Write Defaults ON when you enter Play mode or build your avatar.

## Remove FX Animator Layers

TODO document this

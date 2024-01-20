# Vendored files

Files which I would prefer to maintain by keeping closer to upstream.

If this becomes infeasible, then move back into the main source tree.

Changes made:

- .editorconfig: Set as root so that repository rules are not enforced here
- AnimationUtil.cs: Replace `MA_VRCSDK3_AVATARS` with `NDMMD_VRCSDK3_AVATARS`
- AnimatorCombiner.cs: Replace `MA_VRCSDK3_AVATARS` with `NDMMD_VRCSDK3_AVATARS`
- BlendshapeViewerEditorWindowBase.cs: Remove OnGUI, OnFocus. Make MinWidth, HasGenerationParamsChanged, UsingSkinnedMesh, TryExecuteUpdate protected.

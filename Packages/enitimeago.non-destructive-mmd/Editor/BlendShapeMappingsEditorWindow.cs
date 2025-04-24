using System;
using System.Collections.Generic;
using System.Linq;
using enitimeago.NonDestructiveMMD.vendor.BlendshapeViewer.Scripts.Editor;
using enitimeago.NonDestructiveMMD.vendor.Linguini.Shared.Types.Bundle;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    internal class BlendShapeMappingsEditorWindow : BlendshapeViewerEditorWindowBase
    {
        private const int MMD_MORPHS_PANE_WIDTH = 150;
        private const int SELECTED_BLEND_SHAPES_PANE_WIDTH = 150;

        private CommonChecks _commonChecks;
        // Store a reference to the component rather than the component itself.
        // The component itself, even if annotated with [SerializeField] will not persist when entering play mode.
        private int _dataSource;
        private BlendShapeMappings MappingsComponent => (BlendShapeMappings)EditorUtility.InstanceIDToObject(_dataSource);
        // View-side representation of underlying mapping data for fast accesses.
        private ILookup<int, (string, float)> _knownBlendShapeMappings;

        private int _currentMmdKeyIndex = -1;
        private Vector2 _leftPaneScroll;
        private Vector2 _rightPaneScroll;
        private List<string> _faceBlendShapes = new List<string>();
        private bool _showBlendShapeViewOptions = false;

        // TODO: move into common class?
        private GUIStyle _defaultStyle;
        private GUIStyle _selectedStyle;
        private GUIStyle _existingBlendShapeStyle;
        private GUIStyle _selectedExistingBlendShapeStyle;
        private GUIStyle _hasValueStyle;
        private GUIStyle _selectedHasValueStyle;

        public static void ShowWindow(BlendShapeMappings data)
        {
            var window = GetWindow<BlendShapeMappingsEditorWindow>("Make It MMD");
            window.minSize = new Vector2(640, 480);
            window._dataSource = data.GetInstanceID();
            window.OnGUI();
            window.TryExecuteUpdate();
        }

        public void OnEnable()
        {
            _commonChecks = new CommonChecks(isEditor: true);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        public void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        public void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            // Reload thumbnails on state change. As with any other data, they will be lost on play mode state.
            if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                TryExecuteUpdate();
            }
        }

        public new void OnGUI()
        {
            var selectedBackground = MakeBackgroundTexture(2, 2, new Color(0.5f, 0.5f, 0.5f, 1f));
            var existedBlendShapeBackground = MakeBackgroundTexture(2, 2, new Color(0.2f, 0.3f, 0.6f, 1f));
            var selectedExistingBlendShapeBackground = MakeBackgroundTexture(2, 2, new Color(0.3f, 0.6f, 1f, 1f));
            var hasValueBackground = MakeBackgroundTexture(2, 2, new Color(0.0f, 0.5f, 1f, 1f));
            var selectedHasValueBackground = MakeBackgroundTexture(2, 2, new Color(0.5f, 0.75f, 1f, 1f));

            _defaultStyle = new GUIStyle(GUI.skin.button);
            _selectedStyle = new GUIStyle(GUI.skin.button);
            _selectedStyle.normal.background = selectedBackground;
            _selectedStyle.normal.scaledBackgrounds = new[] { selectedBackground };
            _existingBlendShapeStyle = new GUIStyle(GUI.skin.button);
            _existingBlendShapeStyle.normal.background = existedBlendShapeBackground;
            _existingBlendShapeStyle.normal.scaledBackgrounds = new[] { existedBlendShapeBackground };
            _selectedExistingBlendShapeStyle = new GUIStyle(GUI.skin.button);
            _selectedExistingBlendShapeStyle.normal.background = selectedExistingBlendShapeBackground;
            _selectedExistingBlendShapeStyle.normal.scaledBackgrounds = new[] { selectedExistingBlendShapeBackground };
            _hasValueStyle = new GUIStyle(GUI.skin.button);
            _hasValueStyle.normal.background = hasValueBackground;
            _hasValueStyle.normal.scaledBackgrounds = new[] { hasValueBackground };
            _selectedHasValueStyle = new GUIStyle(GUI.skin.button);
            _selectedHasValueStyle.normal.background = selectedHasValueBackground;
            _selectedHasValueStyle.normal.scaledBackgrounds = new[] { selectedHasValueBackground };

            // TODO: need to refresh if the scene changes
            if (MappingsComponent?.gameObject != null)
            {
                var avatar = MappingsComponent.gameObject.GetComponentInParent<VRCAvatarDescriptor>();
                if (!_commonChecks.RunChecks(avatar, isBuildTime: false))
                {
                    return;
                }
                var visemeSkinnedMesh = avatar.VisemeSkinnedMesh;
                UsingSkinnedMesh(visemeSkinnedMesh);
                _faceBlendShapes.Clear();
                for (int i = 0; i < visemeSkinnedMesh.sharedMesh.blendShapeCount; i++)
                {
                    _faceBlendShapes.Add(visemeSkinnedMesh.sharedMesh.GetBlendShapeName(i));
                }
            }

            // Update view-side mappings from underlying data.
            if (_dataSource != 0)
            {
                var knownMappings = new Dictionary<int, HashSet<(string, float)>>();
                foreach (var (knownMorph, i) in MmdBlendShapeNames.All.Select((value, i) => (value, i)))
                {
                    knownMappings.Add(i, new HashSet<(string, float)>());
                    if (MappingsComponent.blendShapeMappings.TryGetValue(knownMorph.Name, out var selections))
                    {
                        foreach (var selection in selections)
                        {
                            knownMappings[i].Add((selection.Key, selection.Value.scale));
                        }
                    }
                }
                _knownBlendShapeMappings = knownMappings
                    .SelectMany(p => p.Value.Select(x => new { p.Key, Value = x }))
                    .ToLookup(pair => pair.Key, pair => pair.Value);
            }

            GUILayout.BeginHorizontal();

            DrawMmdMorphsPane();
            DrawBlendShapesPane();

            GUILayout.EndHorizontal();
        }

        public new void OnFocus()
        {
            if (!autoUpdateOnFocus) return;
            if (skinnedMesh == null) return;
            if (!HasGenerationParamsChanged()) return;
            TryExecuteUpdate();
        }

        private void DrawMmdMorphsPane()
        {
            GUILayout.BeginVertical("box", GUILayout.Width(MMD_MORPHS_PANE_WIDTH), GUILayout.ExpandHeight(true));

            _leftPaneScroll = GUILayout.BeginScrollView(_leftPaneScroll);

            int i = 0;
            foreach (var grouping in MmdBlendShapeNames.All.GroupBy(x => x.Category))
            {
                GUILayout.Label(grouping.Key.ToString());
                foreach (var blendShape in grouping)
                {
                    bool hasValue = _knownBlendShapeMappings?[i].Count() > 0;
                    var buttonStyle = hasValue ? _hasValueStyle :
                        (_faceBlendShapes.Contains(blendShape.Name) ? _existingBlendShapeStyle : _defaultStyle);
                    if (i == _currentMmdKeyIndex)
                    {
                        buttonStyle = hasValue ? _selectedHasValueStyle :
                            (_faceBlendShapes.Contains(blendShape.Name) ? _selectedExistingBlendShapeStyle : _defaultStyle);
                    }

                    // Hack to have icon next to text until Unity 2023.2
                    // https://forum.unity.com/threads/button-with-icon.733343/#post-9128917
                    var rect = EditorGUILayout.BeginHorizontal();
                    bool hasTwoButtons = hasValue && _faceBlendShapes.Contains(blendShape.Name);
                    if (hasTwoButtons)
                    {
                        // Hack to fill in the gap in-between the two buttons
                        EditorGUI.DrawRect(rect, buttonStyle.normal.background.GetPixel(0, 0));
                    }
                    var existingIcon = EditorGUIUtility.IconContent("d_playLoopOff");
                    bool textButton = GUILayout.Button(blendShape.Name, buttonStyle);
                    bool iconButton = hasTwoButtons ? GUILayout.Button(existingIcon, buttonStyle, GUILayout.MaxWidth(existingIcon.image.width + 12)) : false;
                    EditorGUILayout.EndHorizontal();

                    if (textButton || iconButton)
                    {
                        _currentMmdKeyIndex = i;
                    }
                    i++;
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawBlendShapesPane()
        {
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (_currentMmdKeyIndex >= 0)
            {
                GUILayout.Label(EditorApplication.isPlaying ?
                    L.Tr("MappingsEditorWindow-ViewingBlendShapesForInPlayMode", ("morphName", (FluentString)MmdBlendShapeNames.All[_currentMmdKeyIndex].Name)) :
                    L.Tr("MappingsEditorWindow-EditingBlendShapesFor", ("morphName", (FluentString)MmdBlendShapeNames.All[_currentMmdKeyIndex].Name)));
            }
            else
            {
                GUILayout.Label(L.Tr("MappingsEditorWindow-SelectMMDMorph"));
            }

            GUILayout.BeginHorizontal();
            DrawSelectedBlendShapesPane();
            DrawBlendShapeSelectorPane();
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        }

        private void DrawSelectedBlendShapesPane()
        {
            if (_knownBlendShapeMappings == null)
            {
                return;
            }

            GUILayout.BeginVertical("box", GUILayout.Width(SELECTED_BLEND_SHAPES_PANE_WIDTH), GUILayout.ExpandHeight(true));

            if (_currentMmdKeyIndex >= 0)
            {
                GUILayout.Label(L.Tr("MappingsEditorWindow-SelectedBlendShapes"), EditorStyles.boldLabel);

                foreach ((string avatarKey, float scale) in _knownBlendShapeMappings[_currentMmdKeyIndex])
                {
                    EditorGUILayout.BeginVertical();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(avatarKey);
                    GUILayout.FlexibleSpace();
                    EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                    {
                        if (GUILayout.Button("x"))
                        {
                            Undo.RecordObject(MappingsComponent, "Delete MMD Blend Shape Selection");
                            MappingsComponent.DeleteBlendShapeMapping(MmdBlendShapeNames.All[_currentMmdKeyIndex].Name, avatarKey);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();

                    float newScale = EditorGUILayout.Slider(scale, 0, 1);

                    if (newScale != scale)
                    {
                        Undo.RecordObject(MappingsComponent, "Adjust MMD Blend Shape Scale");
                        MappingsComponent.UpdateBlendShapeMapping(MmdBlendShapeNames.All[_currentMmdKeyIndex].Name, avatarKey, newScale);
                    }

                    EditorGUILayout.EndVertical();
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawBlendShapeSelectorPane()
        {
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(L.Tr("MappingsEditorWindow-AvatarBlendShapes"), EditorStyles.boldLabel);
            GUILayout.FlexibleSpace();
            var settingsButtonContent = EditorGUIUtility.IconContent("_Popup");
            if (GUILayout.Button(settingsButtonContent))
            {
                _showBlendShapeViewOptions = !_showBlendShapeViewOptions;
            }
            EditorGUILayout.EndHorizontal();

            if (_currentMmdKeyIndex >= 0 && _faceBlendShapes.Any())
            {
                if (_showBlendShapeViewOptions)
                {
                    bool newShowDifferences;
                    bool newUseComputeShader;
                    int newThumbnailSize;
                    EditorGUILayout.BeginHorizontal();
                    newShowDifferences = EditorGUILayout.ToggleLeft(L.Tr("MappingsEditorWindow-HighlightDifferences"), showDifferences, GUILayout.ExpandWidth(false));
                    if (newShowDifferences != showDifferences)
                    {
                        showDifferences = newShowDifferences;
                        TryExecuteUpdate();
                    }
                    if (SystemInfo.supportsComputeShaders)
                    {
                        newUseComputeShader = EditorGUILayout.ToggleLeft(L.Tr("MappingsEditorWindow-EnableComputeShader"), useComputeShader, GUILayout.ExpandWidth(false));
                        if (newUseComputeShader != useComputeShader)
                        {
                            useComputeShader = newUseComputeShader;
                            TryExecuteUpdate();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    newThumbnailSize = EditorGUILayout.IntSlider(L.Tr("MappingsEditorWindow-ThumbnailSize"), thumbnailSize, 100, 300);
                    if (newThumbnailSize != thumbnailSize)
                    {
                        thumbnailSize = newThumbnailSize;
                        TryExecuteUpdate();
                    }
                    EditorGUI.BeginDisabledGroup(skinnedMesh == null || AnimationMode.InAnimationMode());
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndHorizontal();
                }

                _rightPaneScroll = GUILayout.BeginScrollView(_rightPaneScroll);

                var width = Mathf.Max(_generatedSize, MinWidth);
                var mod = Mathf.Max(1, (int)(position.width - MMD_MORPHS_PANE_WIDTH - SELECTED_BLEND_SHAPES_PANE_WIDTH) / (width + 12));
                var shown = 0;

                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                {
                    for (int i = 0; i < _faceBlendShapes.Count; i++)
                    {
                        string blendShapeName = _faceBlendShapes[i];
                        var texture2D = i < tex2ds.Length ? tex2ds[i] : null;
                        if (shown % mod == 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.FlexibleSpace();
                        }

                        bool isSelected = _knownBlendShapeMappings != null && _knownBlendShapeMappings[_currentMmdKeyIndex].Any(x => x.Item1 == blendShapeName);

                        var buttonStyle = new GUIStyle(isSelected ? _hasValueStyle : _defaultStyle);
                        buttonStyle.imagePosition = ImagePosition.ImageAbove;
                        var buttonContent = new GUIContent();
                        buttonContent.image = texture2D;
                        buttonContent.text = blendShapeName;
                        if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.Width(width)))
                        {
                            Undo.RecordObject(MappingsComponent, "Add MMD Blend Shape Selection");
                            MappingsComponent.AddBlendShapeMapping(MmdBlendShapeNames.All[_currentMmdKeyIndex].Name, blendShapeName);
                        }

                        if ((shown + 1) % mod == 0 || i == _faceBlendShapes.Count - 1)
                        {
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }

                        shown++;
                    }
                }
                EditorGUI.EndDisabledGroup();

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label(L.Tr("MappingsEditorWindow-SelectMMDMorph"));
            }

            GUILayout.EndVertical();
        }

        private Texture2D MakeBackgroundTexture(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; i++)
            {
                pix[i] = col;
            }
            Texture2D backgroundTexture = new Texture2D(width, height);
            backgroundTexture.SetPixels(pix);
            backgroundTexture.Apply();
            return backgroundTexture;
        }
    }
}

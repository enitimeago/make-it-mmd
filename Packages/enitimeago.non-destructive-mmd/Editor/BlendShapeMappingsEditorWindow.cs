﻿using System;
using System.Collections.Generic;
using System.Linq;
using enitimeago.NonDestructiveMMD.vendor.BlendshapeViewer.Scripts.Editor;
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
            _defaultStyle = new GUIStyle(GUI.skin.button);
            _selectedStyle = new GUIStyle(GUI.skin.button);
            _selectedStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.5f, 0.5f, 0.5f, 1f));
            _existingBlendShapeStyle = new GUIStyle(GUI.skin.button);
            _existingBlendShapeStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.2f, 0.3f, 0.6f, 1f));
            _selectedExistingBlendShapeStyle = new GUIStyle(GUI.skin.button);
            _selectedExistingBlendShapeStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.3f, 0.6f, 1f, 1f));
            _hasValueStyle = new GUIStyle(GUI.skin.button);
            _hasValueStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.0f, 0.5f, 1f, 1f));
            _selectedHasValueStyle = new GUIStyle(GUI.skin.button);
            _selectedHasValueStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.5f, 0.75f, 1f, 1f));

            // TODO: need to refresh if the scene changes
            if (MappingsComponent?.gameObject != null)
            {
                var avatar = MappingsComponent.gameObject.GetComponentInParent<VRCAvatarDescriptor>();
                if (!_commonChecks.RunChecks(avatar))
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
            // TODO: this is really inefficient O(n^2) and ugly code. have the underlying data's Validate() deal with dupes?
            // if that's not possible and this is a bottleneck, will need to persist view-side data and sync with storage.
            // Seems like it's not too slow to rerun in OnGUI loop though.
            if (_dataSource != 0)
            {
                // TODO: technical debt that would be resolved with ISerializationCallbackReceiver this is terrible this is terrible this is terrible
                var knownMappings = new Dictionary<int, (HashSet<string>, Dictionary<string, float>)>();
                var mappingsToSearch = new LinkedList<MMDToAvatarBlendShape>(MappingsComponent.blendShapeMappings);
                foreach (var (knownMorph, i) in MmdBlendShapeNames.All.Select((value, i) => (value, i)))
                {
                    // Iterate all underlying mappings due to risk of duplicate keys.
                    // Use set to avoid potential duplicate values.
                    knownMappings.Add(i, (new HashSet<string>(), new Dictionary<string, float>()));
                    for (var mapping = mappingsToSearch.First; mapping != null; mapping = mapping.Next)
                    {
                        if (mapping.Value.mmdKey == knownMorph.Name)
                        {
                            // TODO: technical debt that would be resolved with ISerializationCallbackReceiver this is terrible this is terrible this is terrible
                            if (mapping.Value.avatarKeyScaleOverrides?.Length > 0)
                            {
                                foreach (var avatarKey in mapping.Value.avatarKeys.Zip(mapping.Value.avatarKeyScaleOverrides, (key, scale) => new { Key = key, Scale = scale }))
                                {
                                    knownMappings[i].Item1.Add(avatarKey.Key);
                                    if (avatarKey.Scale != 1.0f)
                                    {
                                        knownMappings[i].Item2[avatarKey.Key] = avatarKey.Scale;
                                    }
                                }
                            }
                            else
                            {
                                foreach (string avatarKey in mapping.Value.avatarKeys)
                                {
                                    knownMappings[i].Item1.Add(avatarKey);
                                }
                            }
                            mappingsToSearch.Remove(mapping);
                        }
                    }
                }
                // TODO: technical debt that would be resolved with ISerializationCallbackReceiver this is terrible this is terrible this is terrible
                _knownBlendShapeMappings = knownMappings
                    .SelectMany(p => p.Value.Item1.Select(x => new { p.Key, Value = (x, p.Value.Item2.TryGetValue(x, out float scale) ? scale : 1.0f) }))
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
                GUILayout.Label(
                    string.Format(
                        EditorApplication.isPlaying ? L.Tr("MappingsEditorWindow:ViewingBlendShapesForInPlayMode") : L.Tr("MappingsEditorWindow:EditingBlendShapesFor"),
                        MmdBlendShapeNames.All[_currentMmdKeyIndex].Name));
            }
            else
            {
                GUILayout.Label(L.Tr("MappingsEditorWindow:SelectMMDMorph"));
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
                GUILayout.Label(L.Tr("MappingsEditorWindow:SelectedBlendShapes"), EditorStyles.boldLabel);

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
            GUILayout.Label(L.Tr("MappingsEditorWindow:AvatarBlendShapes"), EditorStyles.boldLabel);
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
                    newShowDifferences = EditorGUILayout.ToggleLeft(L.Tr("MappingsEditorWindow:HighlightDifferences"), showDifferences, GUILayout.ExpandWidth(false));
                    if (newShowDifferences != showDifferences)
                    {
                        showDifferences = newShowDifferences;
                        TryExecuteUpdate();
                    }
                    if (SystemInfo.supportsComputeShaders)
                    {
                        newUseComputeShader = EditorGUILayout.ToggleLeft(L.Tr("MappingsEditorWindow:EnableComputeShader"), useComputeShader, GUILayout.ExpandWidth(false));
                        if (newUseComputeShader != useComputeShader)
                        {
                            useComputeShader = newUseComputeShader;
                            TryExecuteUpdate();
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    newThumbnailSize = EditorGUILayout.IntSlider(L.Tr("MappingsEditorWindow:ThumbnailSize"), thumbnailSize, 100, 300);
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
                GUILayout.Label(L.Tr("MappingsEditorWindow:SelectMMDMorph"));
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

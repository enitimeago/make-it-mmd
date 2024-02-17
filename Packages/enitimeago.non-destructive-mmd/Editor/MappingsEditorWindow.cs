using System;
using System.Collections.Generic;
using System.Linq;
using enitimeago.NonDestructiveMMD.vendor.BlendshapeViewer.Scripts.Editor;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    internal class MappingsEditorWindow : BlendshapeViewerEditorWindowBase
    {
        private const int MMD_MORPHS_PANE_WIDTH = 150;
        private const int SELECTED_BLEND_SHAPES_PANE_WIDTH = 150;

        private CommonChecks _commonChecks;
        private BlendShapeMappings _dataSource = null;
        // View-side representation of underlying mapping data for fast accesses.
        private ILookup<int, string> _knownBlendShapeMappings;

        private int _currentMmdKeyIndex = -1;
        private Vector2 _leftPaneScroll;
        private Vector2 _rightPaneScroll;
        private List<string> _faceBlendShapes = new List<string>();
        private bool _showBlendShapeViewOptions = false;

        // TODO: move into common class?
        private GUIStyle _defaultStyle;
        private GUIStyle _selectedStyle;
        private GUIStyle _hasValueStyle;
        private GUIStyle _selectedHasValueStyle;
        private GUIStyle _boldLabelStyle;

        public void OnEnable()
        {
            _commonChecks = new CommonChecks(isEditor: true);
        }

        public static void ShowWindow(BlendShapeMappings data)
        {
            var window = GetWindow<MappingsEditorWindow>("Make It MMD");
            window._dataSource = data;
            window.ReloadMappings();
            window.OnGUI();
            window.TryExecuteUpdate();
        }

        // Update view-side mappings from underlying data.
        // TODO: this is really inefficient O(n^2) and ugly code. have the underlying data's Validate() deal with dupes?
        // if that's not possible and this is a bottleneck, will need to persist view-side data and sync with storage.
        private void ReloadMappings()
        {
            var knownMappings = new Dictionary<int, HashSet<string>>();
            var mappingsToSearch = new LinkedList<MMDToAvatarBlendShape>(_dataSource.blendShapeMappings);
            foreach (var (knownMorph, i) in MmdBlendShapeNames.All.Select((value, i) => (value, i)))
            {
                // Iterate all underlying mappings due to risk of duplicate keys.
                // Use set to avoid potential duplicate values.
                knownMappings.Add(i, new HashSet<string>());
                for (var mapping = mappingsToSearch.First; mapping != null; mapping = mapping.Next)
                {
                    if (mapping.Value.mmdKey == knownMorph.Name)
                    {
                        knownMappings[i].UnionWith(mapping.Value.avatarKeys);
                        mappingsToSearch.Remove(mapping);
                    }
                }
            }
            _knownBlendShapeMappings = knownMappings
                .SelectMany(p => p.Value.Select(x => new { p.Key, Value = x }))
                .ToLookup(pair => pair.Key, pair => pair.Value);
        }

        private void OnGUI()
        {
            _defaultStyle = new GUIStyle(GUI.skin.button);
            _selectedStyle = new GUIStyle(GUI.skin.button);
            _selectedStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.5f, 0.5f, 0.5f, 1f));
            _hasValueStyle = new GUIStyle(GUI.skin.button);
            _hasValueStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.0f, 0.5f, 1f, 1f));
            _selectedHasValueStyle = new GUIStyle(GUI.skin.button);
            _selectedHasValueStyle.normal.background = MakeBackgroundTexture(2, 2, new Color(0.5f, 0.75f, 1f, 1f));
            _boldLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };

            if (_dataSource == null)
            {
                GUILayout.Label(L.Tr("MappingsEditorWindow:NoData"));
                return;
            }

            // TODO: should this be moved into ShowWindow?
            // TODO: need to refresh if the scene changes
            if (_dataSource.gameObject != null)
            {
                var avatar = _dataSource.gameObject.GetComponentInParent<VRCAvatarDescriptor>();
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

            if (_knownBlendShapeMappings != null // TODO: why isn't _dataSource != null good enough?
                && _knownBlendShapeMappings.Count == 0)
            {
                ReloadMappings();
            }

            int i = 0;
            foreach (var grouping in MmdBlendShapeNames.All.GroupBy(x => x.Category))
            {
                GUILayout.Label(grouping.Key.ToString());
                foreach (var blendShape in grouping)
                {
                    var buttonStyle = _knownBlendShapeMappings?[i].Count() > 0 ? _hasValueStyle : _defaultStyle;
                    if (i == _currentMmdKeyIndex)
                    {
                        buttonStyle = _knownBlendShapeMappings?[i].Count() > 0 ? _selectedHasValueStyle : _selectedStyle;
                    }
                    if (GUILayout.Button(blendShape.Name, buttonStyle))
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
                GUILayout.Label(string.Format(L.Tr("MappingsEditorWindow:EditingBlendShapesFor"), MmdBlendShapeNames.All[_currentMmdKeyIndex].Name));
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
            GUILayout.BeginVertical("box", GUILayout.Width(SELECTED_BLEND_SHAPES_PANE_WIDTH), GUILayout.ExpandHeight(true));

            if (_currentMmdKeyIndex >= 0)
            {
                GUILayout.Label(L.Tr("MappingsEditorWindow:SelectedBlendShapes"), _boldLabelStyle);

                foreach (string avatarKey in _knownBlendShapeMappings[_currentMmdKeyIndex])
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label(avatarKey);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("x"))
                    {
                        Debug.Log("Delete blendshape: " + avatarKey);
                        _dataSource.DeleteBlendShapeMapping(MmdBlendShapeNames.All[_currentMmdKeyIndex].Name, avatarKey);
                        ReloadMappings();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }

        private void DrawBlendShapeSelectorPane()
        {
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(L.Tr("MappingsEditorWindow:AvatarBlendShapes"), _boldLabelStyle);
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
                // TODO: implement removing blendshape
                for (int i = 0; i < _faceBlendShapes.Count; i++)
                {
                    string blendShapeName = _faceBlendShapes[i];
                    var texture2D = i < tex2ds.Length ? tex2ds[i] : null;
                    if (shown % mod == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                    }

                    bool isSelected = _knownBlendShapeMappings[_currentMmdKeyIndex].Contains(blendShapeName);

                    var buttonStyle = new GUIStyle(isSelected ? _hasValueStyle : _defaultStyle);
                    buttonStyle.imagePosition = ImagePosition.ImageAbove;
                    var buttonContent = new GUIContent();
                    buttonContent.image = texture2D;
                    buttonContent.text = blendShapeName;
                    if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.Width(width)))
                    {
                        Debug.Log("Add blendshape: " + blendShapeName);
                        _dataSource.AddBlendShapeMapping(MmdBlendShapeNames.All[_currentMmdKeyIndex].Name, blendShapeName);
                        ReloadMappings();
                    }

                    if ((shown + 1) % mod == 0 || i == _faceBlendShapes.Count - 1)
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }

                    shown++;
                }

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

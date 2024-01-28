using CustomLocalization4EditorExtension;
using enitimeago.NonDestructiveMMD.vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Newtonsoft.Json.Linq;

namespace enitimeago.NonDestructiveMMD
{
    public class MappingsEditorWindow : BlendshapeViewerEditorWindowBase
    {
        private CommonAsserts _commonAsserts;
        private BlendShapeMappings _dataSource = null;
        // Local copy of mappings using int => string to avoid recalculating mappings List<MMDToAvatarBlendShape>.
        // This should only be initialized when the window is created.
        // TODO: Support refreshing mappings if they are changed in the inspector.
        // TODO: Consider whether to only mutate the data source and have this be an immutable representation (i.e. a ViewModel-like representation) of the underlying data.
        private Dictionary<int, string> _knownBlendShapeMappings = new Dictionary<int, string>();

        private int _currentMmdKeyIndex = -1;
        private Vector2 _leftPaneScroll;
        private Vector2 _rightPaneScroll;
        private List<string> _faceBlendShapes = new List<string>();

        private GUIStyle _defaultStyle;
        private GUIStyle _selectedStyle;
        private GUIStyle _hasValueStyle;
        private GUIStyle _selectedHasValueStyle;
        
        public void OnEnable()
        {
            _commonAsserts = new CommonAsserts(isEditor: true);
        }

        public static void ShowWindow(BlendShapeMappings data)
        {
            var window = GetWindow<MappingsEditorWindow>("Make It MMD");
            window._dataSource = data;

            // Retrieve blend shape settings that match known MMD keys.
            foreach (var mapping in data.blendShapeMappings)
            {
                foreach ((string knownMmdKey, int i) in MMDBlendShapes.JapaneseNames())
                {
                    if (mapping.mmdKey == knownMmdKey)
                    {
                        window._knownBlendShapeMappings.Add(i, mapping.avatarKey);
                        break;
                    }
                }
            }
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

            if (_dataSource == null)
            {
                GUILayout.Label("No data. Maybe you are in play mode?");
                return;
            }

            // TODO: should this be moved into ShowWindow?
            // TODO: need to refresh if the scene changes
            if (_dataSource.gameObject != null)
            {
                var avatar = _dataSource.gameObject.GetComponentInParent<VRCAvatarDescriptor>();
                if (!_commonAsserts.RunAsserts(avatar))
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

            DrawLeftPane();
            DrawRightPane();

            GUILayout.EndHorizontal();
        }

        private void OnFocus()
        {
            if (!autoUpdateOnFocus) return;
            if (skinnedMesh == null) return;
            if (!HasGenerationParamsChanged()) return;
            TryExecuteUpdate();
        }

        private void DrawLeftPane()
        {
            GUILayout.BeginVertical("box", GUILayout.Width(150), GUILayout.ExpandHeight(true));

            _leftPaneScroll = GUILayout.BeginScrollView(_leftPaneScroll);

            foreach ((string name, int i) in MMDBlendShapes.JapaneseNames())
            {
                var buttonStyle = _knownBlendShapeMappings.ContainsKey(i) ? _hasValueStyle : _defaultStyle;
                if (i == _currentMmdKeyIndex)
                {
                    buttonStyle = _knownBlendShapeMappings.ContainsKey(i) ? _selectedHasValueStyle : _selectedStyle;
                }

                if (GUILayout.Button(name, buttonStyle))
                {
                    _currentMmdKeyIndex = i;
                }
            }

            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        private void DrawRightPane()
        {
            GUILayout.BeginVertical("box", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

            if (_currentMmdKeyIndex >= 0 && _faceBlendShapes.Any())
            {
                GUILayout.Label(string.Format(CL4EE.Tr("MappingsEditorWindow:SelectBlendShapeFor"), MMDBlendShapes.Names[_currentMmdKeyIndex]));

                var italicLabelStyle = new GUIStyle(GUI.skin.label);
                italicLabelStyle.fontStyle = FontStyle.Italic;

                // https://forum.unity.com/threads/how-get-packageinfo-by-name-and-version.761936/#post-9171308
                var blendshapeViewerPackage = AssetDatabase
                    .FindAssets("package")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(x => x.EndsWith("package.json"))
                    .Where(x => AssetDatabase.LoadAssetAtPath<TextAsset>(x) != null)
                    .Select(s => AssetDatabase.LoadAssetAtPath<TextAsset>(s).text)
                    .Select(JObject.Parse)
                    .FirstOrDefault(j => (string)j["name"] == "dev.hai-vr.blendshape-viewer");
                GUILayout.Label($"Avatar previews powered by Blendshape Viewer {(string)blendshapeViewerPackage["version"]}", italicLabelStyle);

                var serializedObject = new SerializedObject(this);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(showDifferences)));
                if (SystemInfo.supportsComputeShaders)
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(useComputeShader)));
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.IntSlider(serializedObject.FindProperty(nameof(thumbnailSize)), 100, 300);

                EditorGUI.BeginDisabledGroup(skinnedMesh == null || AnimationMode.InAnimationMode());
                if (GUILayout.Button("Update"))
                {
                    TryExecuteUpdate();
                }
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();

                _rightPaneScroll = GUILayout.BeginScrollView(_rightPaneScroll);

                string selectedBlendShape;
                _knownBlendShapeMappings.TryGetValue(_currentMmdKeyIndex, out selectedBlendShape);

                var width = Mathf.Max(thumbnailSize, MinWidth);
                var mod = Mathf.Max(1, (int)position.width / (width + 15));
                var shown = 1;
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("None", string.IsNullOrEmpty(selectedBlendShape) ? _hasValueStyle : _defaultStyle))
                {
                    Debug.Log("Unselected blendshape");
                    _knownBlendShapeMappings.Remove(_currentMmdKeyIndex);
                    _dataSource.RemoveBlendShapeMapping(MMDBlendShapes.Names[_currentMmdKeyIndex]);
                }
                for (int i = 0; i < _faceBlendShapes.Count; i++)
                {
                    string blendShapeName = _faceBlendShapes[i];
                    var texture2D = i < tex2ds.Length ? tex2ds[i] : null;
                    if (shown % mod == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                    }

                    var buttonStyle = new GUIStyle(blendShapeName == selectedBlendShape ? _hasValueStyle : _defaultStyle);
                    buttonStyle.imagePosition = ImagePosition.ImageAbove;
	                var buttonContent = new GUIContent();
                    buttonContent.image = texture2D;
                    buttonContent.text = blendShapeName;
                    if (GUILayout.Button(buttonContent, buttonStyle, GUILayout.Width(width - 25)))
                    {
                        Debug.Log("Selected blendshape: " + blendShapeName);
                        _knownBlendShapeMappings[_currentMmdKeyIndex] = blendShapeName;
                        _dataSource.SetBlendShapeMapping(MMDBlendShapes.Names[_currentMmdKeyIndex], blendShapeName);
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
                GUILayout.Label(CL4EE.Tr("MappingsEditorWindow:SelectMMDMorph"));
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

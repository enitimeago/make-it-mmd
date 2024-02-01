﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Newtonsoft.Json.Linq;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    public class MappingsEditorWindow : EditorWindow
    {
        private CommonChecks _commonChecks;
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
            _commonChecks = new CommonChecks(isEditor: true);
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
                if (!_commonChecks.RunChecks(avatar))
                {
                    return;
                }
                var visemeSkinnedMesh = avatar.VisemeSkinnedMesh;
                if (visemeSkinnedMesh == null)
                {
                    GUILayout.Label("Avatar has no face skin mesh!");
                    return;
                }
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

            _rightPaneScroll = GUILayout.BeginScrollView(_rightPaneScroll);

            if (_currentMmdKeyIndex >= 0 && _faceBlendShapes.Any())
            {
                GUILayout.Label(string.Format(L.Tr("MappingsEditorWindow:SelectBlendShapeFor"), MMDBlendShapes.Names[_currentMmdKeyIndex]));

                string selectedBlendShape;
                _knownBlendShapeMappings.TryGetValue(_currentMmdKeyIndex, out selectedBlendShape);

                if (GUILayout.Button("None", string.IsNullOrEmpty(selectedBlendShape) ? _selectedStyle : _defaultStyle))
                {
                    Debug.Log("Unselected blendshape");
                    _knownBlendShapeMappings.Remove(_currentMmdKeyIndex);
                    _dataSource.RemoveBlendShapeMapping(MMDBlendShapes.Names[_currentMmdKeyIndex]);
                }

                foreach (var blendShapeName in _faceBlendShapes)
                {
                    if (GUILayout.Button(blendShapeName, blendShapeName == selectedBlendShape ? _hasValueStyle : _defaultStyle))
                    {
                        Debug.Log("Selected blendshape: " + blendShapeName);
                        _knownBlendShapeMappings[_currentMmdKeyIndex] = blendShapeName;
                        _dataSource.SetBlendShapeMapping(MMDBlendShapes.Names[_currentMmdKeyIndex], blendShapeName);
                    }
                }
            }
            else
            {
                GUILayout.Label(L.Tr("MappingsEditorWindow:SelectMMDMorph"));
            }

            GUILayout.EndScrollView();

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

// Blendshape Viewer
//
// MIT License
//
// Copyright (c) 2023 Haï~ (@vr_hai github.com/hai-vr)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace enitimeago.NonDestructiveMMD.vendor.BlendshapeViewer.Scripts.Editor
{
    internal abstract class BlendshapeViewerEditorWindowBase : EditorWindow
    {
        protected const int MinWidth = 150;
        public SkinnedMeshRenderer skinnedMesh;
        public bool showDifferences = true;
        public bool autoUpdateOnFocus = true;
        public int thumbnailSize = 100;
        public bool showHotspots;
        public bool useComputeShader = true;
        public Texture2D[] tex2ds = new Texture2D[0];

        protected string _search;
        
        protected Vector2 _scrollPos;
        protected SkinnedMeshRenderer _generatedFor;
        protected int _generatedSize;

        protected Vector3 _generatedTransformPosition;
        protected Quaternion _generatedTransformRotation;
        protected float _generatedFieldOfView;
        protected bool _generatedOrthographic;
        protected float _generatedNearClipPlane;
        protected float _generatedFarClipPlane;
        protected float _generatedOrthographicSize;
        protected Rect m_area;
        
        protected const string SearchLabel = "Search";

        public BlendshapeViewerEditorWindowBase()
        {
            titleContent = new GUIContent("BlendshapeViewer");
        }

        protected void OnFocus()
        {
            if (!autoUpdateOnFocus) return;
            if (skinnedMesh == null) return;
            if (!HasGenerationParamsChanged()) return;

            TryExecuteUpdate();
        }

        protected void OnGUI()
        {
            var serializedObject = new SerializedObject(this);
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(skinnedMesh)));
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(showDifferences)));
            if (showDifferences)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(showHotspots)));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(autoUpdateOnFocus)));
            if (SystemInfo.supportsComputeShaders)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(useComputeShader)));
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.IntSlider(serializedObject.FindProperty(nameof(thumbnailSize)), 100, 300);

            
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(skinnedMesh == null || AnimationMode.InAnimationMode());
            if (GUILayout.Button("Update"))
            {
                TryExecuteUpdate();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.LabelField("", GUILayout.Width(25));
            EditorGUILayout.LabelField(SearchLabel, GUILayout.Width(50));
            _search = EditorGUILayout.TextField(_search, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
            
            serializedObject.ApplyModifiedProperties();

            var width = Mathf.Max(_generatedSize, MinWidth);
            if (skinnedMesh != null && skinnedMesh.sharedMesh != null && skinnedMesh.sharedMesh.blendShapeCount > 0 && _generatedFor == skinnedMesh)
            {
                var total = skinnedMesh.sharedMesh.blendShapeCount;
                var serializedSkinnedMesh = new SerializedObject(skinnedMesh);

                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(position.height - EditorGUIUtility.singleLineHeight * 7));
                
                var hasSearch = !string.IsNullOrEmpty(_search);

                var mod = Mathf.Max(1, (int)position.width / (width + 15));
                var highlightColor = EditorGUIUtility.isProSkin ? new Color(0.92f, 0.62f, 0.25f) : new Color(0.74f, 0.47f, 0.1f);
                var shown = 0;
                var clipboard = EditorGUIUtility.IconContent("Clipboard").image;
                for (var index = 0; index < total; index++)
                {
                    var blendShapeName = skinnedMesh.sharedMesh.GetBlendShapeName(index);
                    if (hasSearch && !IsMatch(blendShapeName))
                    {
                        if (index == total - 1)
                        {
                            GUILayout.FlexibleSpace();
                            EditorGUILayout.EndHorizontal();
                        }
                        continue;
                    }
                    
                    var texture2D = index < tex2ds.Length ? tex2ds[index] : null;
                    if (shown % mod == 0)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.FlexibleSpace();
                    }

                    EditorGUILayout.BeginVertical();
                    GUILayout.Box(texture2D);
                    var weight = serializedSkinnedMesh.FindProperty("m_BlendShapeWeights").GetArrayElementAtIndex(index);
                    var isNonZero = weight.floatValue > 0f;
                    EditorGUILayout.BeginHorizontal();
                    Colored(isNonZero, highlightColor, () =>
                    {
                        EditorGUILayout.TextField(blendShapeName, isNonZero ? EditorStyles.boldLabel : EditorStyles.label, GUILayout.Width(width - 25));
                    });
                    if (GUILayout.Button(new GUIContent(clipboard, $"Copy {blendShapeName} to Clipboard"), GUILayout.Width(25)))
                    {
                        GUIUtility.systemCopyBuffer = blendShapeName;
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Slider(weight, 0f, 100f, GUIContent.none, GUILayout.Width(width));
                    EditorGUILayout.EndVertical();

                    if ((shown + 1) % mod == 0 || index == total - 1)
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.EndHorizontal();
                    }

                    shown++;
                }

                GUILayout.EndScrollView();
                serializedSkinnedMesh.ApplyModifiedProperties();
            }
        }

        protected void TryExecuteUpdate()
        {
            if (AnimationMode.InAnimationMode()) return;

            Generate();
            SaveGenerationParams();
        }

        protected void SaveGenerationParams()
        {
            _generatedFor = skinnedMesh;
            _generatedSize = thumbnailSize;

            var sceneCamera = SceneView.lastActiveSceneView.camera;
            _generatedTransformPosition = sceneCamera.transform.position;
            _generatedTransformRotation = sceneCamera.transform.rotation;
            var whRatio = (1f * sceneCamera.pixelWidth / sceneCamera.pixelHeight);
            _generatedFieldOfView = whRatio < 1 ? sceneCamera.fieldOfView * whRatio : sceneCamera.fieldOfView;
            _generatedOrthographic = sceneCamera.orthographic;
            _generatedNearClipPlane = sceneCamera.nearClipPlane;
            _generatedFarClipPlane = sceneCamera.farClipPlane;
            _generatedOrthographicSize = sceneCamera.orthographicSize;
        }

        protected bool HasGenerationParamsChanged()
        {
            var sceneCamera = SceneView.lastActiveSceneView.camera;
            if (_generatedTransformPosition != sceneCamera.transform.position) return true;
            if (_generatedTransformRotation != sceneCamera.transform.rotation) return true;
            var whRatio = (1f * sceneCamera.pixelWidth / sceneCamera.pixelHeight);
            if (Math.Abs(_generatedFieldOfView - (whRatio < 1 ? sceneCamera.fieldOfView * whRatio : sceneCamera.fieldOfView)) > 0.001f) return true;
            if (_generatedOrthographic != sceneCamera.orthographic) return true;
            if (Math.Abs(_generatedNearClipPlane - sceneCamera.nearClipPlane) > 0.001f) return true;
            if (Math.Abs(_generatedFarClipPlane - sceneCamera.farClipPlane) > 0.001f) return true;
            if (Math.Abs(_generatedOrthographicSize - sceneCamera.orthographicSize) > 0.001f) return true;
            return false;
        }

        protected void UsingSkinnedMesh(SkinnedMeshRenderer inSkinnedMesh)
        {
            skinnedMesh = inSkinnedMesh;
        }

        protected void Generate()
        {
            var module = new BlendshapeViewerGenerator();
            try
            {
                module.Begin(skinnedMesh, showHotspots ? 0.95f : 0, useComputeShader);
                Texture2D neutralTexture = null;
                if (showDifferences)
                {
                    neutralTexture = NewTexture();
                    module.Render(EmptyClip(), neutralTexture);
                }

                var results = new [] {skinnedMesh}
                    .SelectMany(relevantSmr =>
                    {
                        var sharedMesh = relevantSmr.sharedMesh;

                        return Enumerable.Range(0, sharedMesh.blendShapeCount)
                            .Select(i =>
                            {
                                var blendShapeName = sharedMesh.GetBlendShapeName(i);
                                var currentWeight = relevantSmr.GetBlendShapeWeight(i);

                                // If the user has already animated this to 100, in normal circumstances the diff would show nothing.
                                // Animate the blendshape to 0 instead so that a diff can be generated.
                                var isAlreadyAnimatedTo100 = Math.Abs(currentWeight - 100f) < 0.001f;
                                var tempClip = new AnimationClip();
                                AnimationUtility.SetEditorCurve(
                                    tempClip,
                                    new EditorCurveBinding
                                    {
                                        path = "",
                                        type = typeof(SkinnedMeshRenderer),
                                        propertyName = $"blendShape.{blendShapeName}"
                                    },
                                    AnimationCurve.Constant(0, 1 / 60f, isAlreadyAnimatedTo100 ? 0f : 100f)
                                );

                                return tempClip;
                            })
                            .ToArray();
                    })
                    .ToArray();

                tex2ds = results
                    .Select((clip, i) =>
                    {
                        if (i % 10 == 0) EditorUtility.DisplayProgressBar("Rendering", $"Rendering ({i} / {results.Length})", 1f * i / results.Length);

                        var currentWeight = skinnedMesh.GetBlendShapeWeight(i);
                        var isAlreadyAnimatedTo100 = Math.Abs(currentWeight - 100f) < 0.001f;

                        var result = NewTexture();
                        module.Render(clip, result);
                        if (i == 0)
                        {
                            // Workaround a weird bug where the first blendshape is always incorrectly rendered
                            module.Render(clip, result);
                        }
                        if (showDifferences)
                        {
                            if (isAlreadyAnimatedTo100)
                            {
                                module.Diff(neutralTexture, result, result);
                            }
                            else
                            {
                                module.Diff(result, neutralTexture, result);
                            }
                        }
                        return result;
                    })
                    .ToArray();
            }
            finally
            {
                module.Terminate();
                EditorUtility.ClearProgressBar();
            }
        }

        protected static void Colored(bool isActive, Color bgColor, Action inside)
        {
            var col = GUI.contentColor;
            try
            {
                if (isActive) GUI.contentColor = bgColor;
                inside();
            }
            finally
            {
                GUI.contentColor = col;
            }
        }

        protected static AnimationClip EmptyClip()
        {
            var emptyClip = new AnimationClip();
            AnimationUtility.SetEditorCurve(
                emptyClip,
                new EditorCurveBinding
                {
                    path = "_ignored",
                    type = typeof(GameObject),
                    propertyName = "m_Active"
                },
                AnimationCurve.Constant(0, 1 / 60f, 100f)
            );
            return emptyClip;
        }

        protected Texture2D NewTexture()
        {
            var newTexture = new Texture2D(Mathf.Max(thumbnailSize, MinWidth), thumbnailSize, TextureFormat.RGB24, false);
            newTexture.wrapMode = TextureWrapMode.Clamp;
            return newTexture;
        }
        
        protected bool IsMatch(string thatName)
        {
            var propertyName = thatName.ToLowerInvariant();
            return _search.ToLowerInvariant().Split(' ').All(needle => propertyName.Contains(needle));
        }

        

        

        protected static BlendshapeViewerEditorWindowBase Obtain()
        {
            var editor = GetWindow<BlendshapeViewerEditorWindowBase>(false, null, false);
            editor.titleContent = new GUIContent("BlendshapeViewer");
            return editor;
        }
    }
}
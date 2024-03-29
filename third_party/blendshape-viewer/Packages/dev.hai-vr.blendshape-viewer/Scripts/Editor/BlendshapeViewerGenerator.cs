﻿// Blendshape Viewer
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
using UnityEditor;
using UnityEngine;

namespace enitimeago.NonDestructiveMMD.vendor.BlendshapeViewer.Scripts.Editor
{
    internal class BlendshapeViewerGenerator
    {
        private Material _material;
        private SkinnedMeshRenderer _skinnedMesh;
        private bool _useComputeShader;
        private Camera _camera;
        private float _overlay;
        private BlendshapeViewerDiffCompute _diffCompute;

        public void Begin(SkinnedMeshRenderer skinnedMesh, float overlay, bool useComputeShader)
        {
            _skinnedMesh = skinnedMesh;
            _overlay = overlay;
            _useComputeShader = SystemInfo.supportsComputeShaders && useComputeShader;

            _material = new Material(_useComputeShader ? Shader.Find("enitimeago/MakeItMMD_Hai_BlendshapeViewerRectOnly") : Shader.Find("enitimeago/MakeItMMD_Hai_BlendshapeViewer"));
            _material.SetFloat("_Hotspots", _overlay);
            _camera = new GameObject().AddComponent<Camera>();

            var sceneCamera = SceneView.lastActiveSceneView.camera;
            _camera.transform.position = sceneCamera.transform.position;
            _camera.transform.rotation = sceneCamera.transform.rotation;
            var whRatio = (1f * sceneCamera.pixelWidth / sceneCamera.pixelHeight);
            _camera.fieldOfView = whRatio < 1 ? sceneCamera.fieldOfView * whRatio : sceneCamera.fieldOfView;
            _camera.orthographic = sceneCamera.orthographic;
            _camera.nearClipPlane = sceneCamera.nearClipPlane;
            _camera.farClipPlane = sceneCamera.farClipPlane;
            _camera.orthographicSize = sceneCamera.orthographicSize;

            if (_useComputeShader)
            {
                _diffCompute = new BlendshapeViewerDiffCompute();
            }
        }

        public void Terminate()
        {
            Object.DestroyImmediate(_material);
            Object.DestroyImmediate(_camera.gameObject);
            if (_useComputeShader)
            {
                _diffCompute.Terminate();
            }
        }

        public void Render(AnimationClip clip, Texture2D element)
        {
            try
            {
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_skinnedMesh.gameObject, clip, 1 / 60f);
                AnimationMode.EndSampling();

                var renderTexture = RenderTexture.GetTemporary(element.width, element.height, 24);
                renderTexture.wrapMode = TextureWrapMode.Clamp;

                RenderCamera(renderTexture, _camera);
                RenderTextureTo(renderTexture, element);
                RenderTexture.ReleaseTemporary(renderTexture);
            }
            finally
            {
                AnimationMode.StopAnimationMode();
            }
        }

        private static void RenderCamera(RenderTexture renderTexture, Camera camera)
        {
            var originalRenderTexture = camera.targetTexture;
            var originalAspect = camera.aspect;
            try
            {
                camera.targetTexture = renderTexture;
                camera.aspect = (float) renderTexture.width / renderTexture.height;
                camera.Render();
            }
            finally
            {
                camera.targetTexture = originalRenderTexture;
                camera.aspect = originalAspect;
            }
        }

        private static void RenderTextureTo(RenderTexture renderTexture, Texture2D texture2D)
        {
            RenderTexture.active = renderTexture;
            texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            texture2D.Apply();
            RenderTexture.active = null;
        }

        public void Diff(Texture2D source, Texture2D neutralTexture, Texture2D newTexture)
        {
            var diff = RenderTexture.GetTemporary(newTexture.width, newTexture.height, 24);
            _material.SetTexture("_NeutralTex", neutralTexture);
            if (_useComputeShader)
            {
                _material.SetVector("_Rect", _diffCompute.Compute(source, neutralTexture));
            }
            Graphics.Blit(source, diff, _material);
            RenderTextureTo(diff, newTexture);
            RenderTexture.ReleaseTemporary(diff);
        }
    }
}
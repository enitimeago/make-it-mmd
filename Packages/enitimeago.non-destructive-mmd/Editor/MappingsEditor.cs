﻿using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    [CustomEditor(typeof(BlendShapeMappings))]
    public class MappingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var data = (BlendShapeMappings)target;

            if (GUILayout.Button("Open Editor"))
            {
                MappingsEditorWindow.ShowWindow(data);
            }
        }
    }
}
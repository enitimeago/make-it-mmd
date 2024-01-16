using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    [CustomEditor(typeof(NonDestructiveMMD))]
    public class NonDestructiveMMDEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            NonDestructiveMMD data = (NonDestructiveMMD)target;

            if (GUILayout.Button("Open Editor"))
            {
                NonDestructiveMMDEditorWindow.ShowWindow(data);
            }
        }
    }
}

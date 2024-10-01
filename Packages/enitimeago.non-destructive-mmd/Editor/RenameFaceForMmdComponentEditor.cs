using Linguini.Shared.Types.Bundle;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    [CustomEditor(typeof(RenameFaceForMmdComponent))]
    public class RenameFaceForMmdComponentEditor : Editor
    {
        private CommonChecks _commonChecks;

        public void OnEnable()
        {
            _commonChecks = new CommonChecks(isEditor: true);
        }

        public override void OnInspectorGUI()
        {
            var data = (RenameFaceForMmdComponent)target;
            var avatar = data.GetComponentInParent<VRCAvatarDescriptor>();

            EditorGUILayout.BeginHorizontal();
            L.DrawLanguagePicker();
            EditorGUILayout.EndHorizontal();

            // Run asserts, however continue rendering GUI if errors are encountered.
            _commonChecks.RunChecks(avatar, isBuildTime: false);
            if (avatar.VisemeSkinnedMesh.name == null)
            {
                return;
            }

            if (!EditorApplication.isPlaying)
            {
                // Let the user know what will happen.
                if (avatar.VisemeSkinnedMesh.name == "Body")
                {
                    EditorGUILayout.HelpBox(L.Tr("RenameFaceForMmdComponentEditor-AlreadyCalledBody"), MessageType.Info);
                }
                else
                {
                    GUILayout.Label(L.Tr("RenameFaceForMmdComponentEditor-ActionToPerform", ("currentName", (FluentString)avatar.VisemeSkinnedMesh.name)), EditorStyles.wordWrappedLabel);

                    var toRenames = RenameFaceForMmdPass.DetermineRenames(avatar.GetComponentsInChildren<SkinnedMeshRenderer>());
                    if (toRenames.Count > 0)
                    {
                        GUILayout.Label(L.Tr("RenameFaceForMmdComponentEditor-ActionToPerformHasConflicts"), EditorStyles.wordWrappedLabel);
                    }
                    GUILayout.Label(L.Tr("RenameFaceForMmdComponentEditor-ActionToPerformSuffix"), EditorStyles.wordWrappedLabel);

                    // TODO: can this be a table so that the columns are the same width?
                    EditorGUI.indentLevel++;
                    {
                        foreach (var toRename in toRenames)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Label(string.Format("{0} ←", toRename.newName), EditorStyles.wordWrappedLabel);
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.ObjectField(toRename.skinnedMeshRenderer, typeof(GameObject), true);
                            EditorGUI.EndDisabledGroup();
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("Body ←", EditorStyles.wordWrappedLabel);
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(avatar.VisemeSkinnedMesh, typeof(GameObject), true);
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
            }
            else
            {
                GUILayout.Label(L.Tr("RenameFaceForMmdComponentEditor-IsPlaying"), EditorStyles.wordWrappedLabel);
            }
        }
    }
}

using CustomLocalization4EditorExtension;
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
    [CustomEditor(typeof(BlendShapeMappings))]
    public class MappingsEditor : Editor
    {
        private bool _showStoredData = false;

        public override void OnInspectorGUI()
        {
            var data = (BlendShapeMappings)target;
            var avatar = data.GetComponentInParent<VRCAvatarDescriptor>();

            EditorGUILayout.BeginHorizontal();
            CL4EE.DrawLanguagePicker();
            EditorGUILayout.EndHorizontal();

            // TODO: unify checks with plugin and editorwindow?
            if (avatar == null)
            {
                EditorGUILayout.HelpBox("This component needs to be placed on or inside an avatar to work!", MessageType.Warning);
            }

            var visemeSkinnedMesh = avatar.VisemeSkinnedMesh;
            if (visemeSkinnedMesh == null)
            {
                EditorGUILayout.HelpBox("Avatar has no face skin mesh!", MessageType.Warning);
            }
            if (visemeSkinnedMesh.name != "Body")
            {
                EditorGUILayout.HelpBox("This component needs your avatar's face mesh to be called \"Body\" to work!", MessageType.Warning);
            }
            for (int i = 0; i < visemeSkinnedMesh.sharedMesh.blendShapeCount; i++)
            {
                string blendShapeName = visemeSkinnedMesh.sharedMesh.GetBlendShapeName(i);
                if (MMDBlendShapes.JapaneseNames().Any(blendShape => blendShape.name == blendShapeName))
                {
                    EditorGUILayout.HelpBox("Avatars with pre-existing MMD blend shapes are unsupported!", MessageType.Warning);
                    break;
                }
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(CL4EE.Tr("MappingsEditor:OpenEditor")))
            {
                MappingsEditorWindow.ShowWindow(data);
            }

            // Extras menu
            if (GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(CL4EE.Tr("MappingsEditor:ShareAsUnitypackage")), false, () => ExportPackage(data));
                menu.AddSeparator("");
                menu.AddItem(new GUIContent(CL4EE.Tr("MappingsEditor:ShowStoredData")), _showStoredData, () => _showStoredData = !_showStoredData);
                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();

            if (_showStoredData)
            {
                var mappings = serializedObject.FindProperty(nameof(BlendShapeMappings.blendShapeMappings));
                EditorGUILayout.PropertyField(mappings);
            }
        }

        private void ExportPackage(BlendShapeMappings mappingsComponent)
        {
            string packagePath = EditorUtility.SaveFilePanel(
                CL4EE.Tr("SaveFilePanel:SaveUnitypackage"),
                "",
                "",
                "unitypackage");

            if (string.IsNullOrEmpty(packagePath))
            {
                return;
            }
            
            // Create a new GameObject with a clone of current mappings.
            // The name doesn't matter because it will match the prefab's name.
            // TODO: implement a copyinto method, or see if it's possible to clone the BlendShapeMappings object and add it into the new GameObject.
            var newGameObject = new GameObject();
            var newMappingsComponent = newGameObject.AddComponent<BlendShapeMappings>();
            newMappingsComponent.blendShapeMappings.AddRange(mappingsComponent.blendShapeMappings);

            // Ensure a unique path for the prefab.
            // TODO: choose a temp folder so that this doesn't turn into "MakeItMMD 1" etc. if it already exists?
            string prefabPath = AssetDatabase.GenerateUniqueAssetPath("Assets/MakeItMMD.prefab");

            // Save the prefab.
            bool prefabSuccess;
            PrefabUtility.SaveAsPrefabAsset(newGameObject, prefabPath, out prefabSuccess);
            if (!prefabSuccess)
            {
                // TODO: see if this can be presented as a dialog.
                Debug.Log("Prefab failed to save");
                return;
            }

            // Export the prefab as a package.
            AssetDatabase.ExportPackage(prefabPath, packagePath);

            // Delete the prefab.
            // This is safe because AssetDatabase.GenerateUniqueAssetPath was used.
            AssetDatabase.DeleteAsset(prefabPath);
        }
    }
}

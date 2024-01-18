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
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            var data = (BlendShapeMappings)target;

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Open Editor"))
            {
                MappingsEditorWindow.ShowWindow(data);
            }

            // Extras menu
            if (GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Share as .unitypackage..."), false, () => ExportPackage(data));
                menu.ShowAsContext();
            }

            EditorGUILayout.EndHorizontal();
        }

        private void ExportPackage(BlendShapeMappings mappingsComponent)
        {
            string packagePath = EditorUtility.SaveFilePanel(
                "Save .unitypackage",
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

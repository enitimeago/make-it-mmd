using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    [CustomEditor(typeof(BlendShapeMappings))]
    public class MappingsEditor : Editor
    {
        private CommonChecks _commonChecks;
        private bool _showStoredData = false;
        
        public void OnEnable()
        {
            _commonChecks = new CommonChecks(isEditor: true);
        }

        public override void OnInspectorGUI()
        {
            var data = (BlendShapeMappings)target;
            var avatar = data.GetComponentInParent<VRCAvatarDescriptor>();

            EditorGUILayout.BeginHorizontal();
            L.DrawLanguagePicker();
            EditorGUILayout.EndHorizontal();

            // Run asserts, however continue rendering GUI if errors are encountered.
            bool avatarOkay = _commonChecks.RunChecks(data) && _commonChecks.RunChecks(avatar);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(L.Tr("MappingsEditor:OpenEditor")))
            {
                MappingsEditorWindow.ShowWindow(data);
            }

            // Extras menu
            if (GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(20)))
            {
                var menu = new GenericMenu();
                if (avatarOkay)
                {
                    menu.AddItem(new GUIContent(L.Tr("MappingsEditor:ShareAsUnitypackage")), false, () => ExportPackage(data));
                    menu.AddSeparator("");
                }
                menu.AddItem(new GUIContent(L.Tr("MappingsEditor:ShowStoredData")), _showStoredData, () => _showStoredData = !_showStoredData);
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
                L.Tr("SaveFilePanel:SaveUnitypackage"),
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

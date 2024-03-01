using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using tar_cs;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    [CustomEditor(typeof(BlendShapeMappings))]
    public class BlendShapeMappingsEditor : Editor
    {
        private CommonChecks _commonChecks;
        private bool _showStoredData = false;
        private float _hasMmdShapeKeysHelpBoxHeight = 0;

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

            bool hasMmdShapeKeys = false;
            if (!EditorApplication.isPlaying)
            {
                // TODO: check all languages
                var visemeSkinnedMesh = avatar?.VisemeSkinnedMesh;
                for (int i = 0; i < visemeSkinnedMesh?.sharedMesh.blendShapeCount; i++)
                {
                    string blendShapeName = visemeSkinnedMesh.sharedMesh.GetBlendShapeName(i);
                    hasMmdShapeKeys = hasMmdShapeKeys || MmdBlendShapeNames.All.Any(blendShape => blendShape.Name == blendShapeName);
                }
            }
            if (hasMmdShapeKeys)
            {
                EditorGUILayout.BeginHorizontal();
                var helpBoxRect = EditorGUILayout.BeginVertical();
                EditorGUILayout.HelpBox(L.Tr("MappingsEditor:ImportBlendShapesSuggestion"), MessageType.Info);
                EditorGUILayout.EndVertical();
                _hasMmdShapeKeysHelpBoxHeight = helpBoxRect.height > 2 ? helpBoxRect.height - 2 : _hasMmdShapeKeysHelpBoxHeight;
                if (GUILayout.Button(L.Tr("MappingsEditor:ImportBlendShapesButton"), GUILayout.MinHeight(_hasMmdShapeKeysHelpBoxHeight)))
                {
                    MmdScanAndImportWindow.ShowWindow(avatar);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(L.Tr("MappingsEditor:OpenEditor")))
            {
                BlendShapeMappingsEditorWindow.ShowWindow(data);
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
            //AssetDatabase.ExportPackage(prefabPath, packagePath);
            using (var outFile = File.Create(packagePath))
            {
                using (var outStream = new GZipStream(outFile, CompressionMode.Compress))
                {
                    using (var writer = new TarWriter(outStream))
                    {
                        string prefabGuid = AssetDatabase.AssetPathToGUID(prefabPath);
                        using (var prefabStream = File.OpenRead(prefabPath))
                        {
                            writer.Write(prefabStream, prefabStream.Length, $"{prefabGuid}/asset");
                        }
                        using (var prefabMetaStream = File.OpenRead(AssetDatabase.GetTextMetaFilePathFromAssetPath(prefabPath)))
                        {
                            writer.Write(prefabMetaStream, prefabMetaStream.Length, $"{prefabGuid}/asset.meta");
                        }
                        using (var prefabPathStream = new MemoryStream(Encoding.ASCII.GetBytes(prefabPath)))
                        {
                            writer.Write(prefabPathStream, prefabPathStream.Length, $"{prefabGuid}/pathname");
                        }

                        // And then include VPAI
                        string vpaiGuid = "93e23fe9bbc86463a9790ebfd1fef5eb";
                        using (var vpaiStream = File.OpenRead("Packages/enitimeago.non-destructive-mmd/Editor/vendor/com.anatawa12.vpm-package-auto-installer.dll"))
                        {
                            writer.Write(vpaiStream, vpaiStream.Length, $"{vpaiGuid}/asset");
                        }
                        using (var vpaiMetaStream = File.OpenRead("Packages/enitimeago.non-destructive-mmd/Editor/vendor/com.anatawa12.vpm-package-auto-installer.dll.meta.txt"))
                        {
                            writer.Write(vpaiMetaStream, vpaiMetaStream.Length, $"{vpaiGuid}/asset.meta");
                        }
                        using (var vpaiPathStream = new MemoryStream(Encoding.ASCII.GetBytes("Assets/com.anatawa12.vpm-package-auto-installer/com.anatawa12.vpm-package-auto-installer.dll")))
                        {
                            writer.Write(vpaiPathStream, vpaiPathStream.Length, $"{vpaiGuid}/pathname");
                        }
                        string configGuid = "9028b92d14f444e2b8c389be130d573f";
                        string configFile = @"{
  ""vpmRepositories"": [
    ""https://enitimeago.github.io/vpm-repos/index.json""
  ],
  ""vpmDependencies"": {
    ""enitimeago.non-destructive-mmd"": ""^0.7.0""
  }
}";
                        string configMeta = @"fileFormatVersion: 2
guid: 9028b92d14f444e2b8c389be130d573f
TextScriptImporter:
  externalObjects: {}
  userData: 
  assetBundleName: 
  assetBundleVariant: 
";
                        using (var configStream = new MemoryStream(Encoding.ASCII.GetBytes(configFile)))
                        {
                            writer.Write(configStream, configStream.Length, $"{configGuid}/asset");
                        }
                        using (var configMetaStream = new MemoryStream(Encoding.ASCII.GetBytes(configMeta)))
                        {
                            writer.Write(configMetaStream, configMetaStream.Length, $"{configGuid}/asset.meta");
                        }
                        using (var configPathStream = new MemoryStream(Encoding.ASCII.GetBytes("Assets/com.anatawa12.vpm-package-auto-installer/config.json")))
                        {
                            writer.Write(configPathStream, configPathStream.Length, $"{configGuid}/pathname");
                        }
                        string folderGuid = "4b344df74d4849e3b2c978b959abd31b";
                        string folderMeta = @"fileFormatVersion: 2
guid: 4b344df74d4849e3b2c978b959abd31b
timeCreated: 1652316538
";
                        using (var folderMetaStream = new MemoryStream(Encoding.ASCII.GetBytes(folderMeta)))
                        {
                            writer.Write(folderMetaStream, folderMetaStream.Length, $"{folderGuid}/asset.meta");
                        }
                        using (var folderPathStream = new MemoryStream(Encoding.ASCII.GetBytes("Assets/com.anatawa12.vpm-package-auto-installer")))
                        {
                            writer.Write(folderPathStream, folderPathStream.Length, $"{folderGuid}/pathname");
                        }
                    }
                }
            }

            // Delete the prefab.
            // This is safe because AssetDatabase.GenerateUniqueAssetPath was used.
            AssetDatabase.DeleteAsset(prefabPath);
        }
    }
}

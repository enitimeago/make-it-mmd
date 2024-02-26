using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    internal class MmdScanAndImportWindow : EditorWindow
    {
        private VRCAvatarDescriptor _avatar = null;
        private bool _doneSearch = false;
        private bool _replaceExisting = false;
        private bool _chooseImportDestination = false;
        private GameObject _customImportDestination = null;
        private Vector2 _blendShapeScrollPosition;
        private List<(string, int)> _mmdBlendShapes = new List<(string, int)>();
        private List<(string, int)> _nonMmdBlendShapes = new List<(string, int)>();
        private Dictionary<string, string> _mmdToNonMmdBlendShapeMappings = new Dictionary<string, string>();

        [MenuItem("Tools/MMD Scan and Import")]
        public static void ShowWindow()
        {
            GetWindow<MmdScanAndImportWindow>("MMD Scan and Import");
        }

        public static void ShowWindow(VRCAvatarDescriptor avatar)
        {
            var window = GetWindow<MmdScanAndImportWindow>("MMD Scan and Import");
            window._avatar = avatar;
        }

        public void OnGUI()
        {
            L.DrawLanguagePicker();

            _avatar = (VRCAvatarDescriptor)EditorGUILayout.ObjectField(L.Tr("MmdScanAndImportWindow:AvatarToScanField"), _avatar, typeof(VRCAvatarDescriptor), true);
            EditorGUI.BeginDisabledGroup(_avatar == null);
            if (GUILayout.Button(_avatar == null ? L.Tr("MmdScanAndImportWindow:ScanButtonNoValidAvatar") : L.Tr("MmdScanAndImportWindow:ScanButton")))
            {
                Scan();
            }
            EditorGUI.EndDisabledGroup();
            if (_doneSearch)
            {
                int totalBlendShapes = _mmdBlendShapes.Count + _nonMmdBlendShapes.Count;
                var mappingsComponent = _avatar.GetComponentInChildren<BlendShapeMappings>();
                var existingMappings = mappingsComponent == null ? new List<KeyValuePair<string, string>>()
                    : _mmdToNonMmdBlendShapeMappings.Where(mapping => mappingsComponent.HasBlendShapeMappings(mapping.Key)).ToList();
                var newMappings = mappingsComponent == null ? _mmdToNonMmdBlendShapeMappings.ToList()
                    : _mmdToNonMmdBlendShapeMappings.Where(mapping => !mappingsComponent.HasBlendShapeMappings(mapping.Key)).ToList();

                GUILayout.Label(string.Format(L.Tr("MmdScanAndImportWindow:ScanReport"), totalBlendShapes, _mmdToNonMmdBlendShapeMappings.Count, _mmdBlendShapes.Count));
                _blendShapeScrollPosition = EditorGUILayout.BeginScrollView(_blendShapeScrollPosition);
                foreach (var entry in _mmdToNonMmdBlendShapeMappings)
                {
                    GUILayout.Label($"{entry.Key} = {entry.Value}");
                }
                if (_mmdToNonMmdBlendShapeMappings.Count < _mmdBlendShapes.Count)
                {
                    GUILayout.Label(L.Tr("MmdScanAndImportWindow:NotFoundHeading"));
                    foreach (var unknown in _mmdBlendShapes.Where(blendShape => !_mmdToNonMmdBlendShapeMappings.ContainsKey(blendShape.Item1)))
                    {
                        GUILayout.Label(unknown.Item1);
                    }
                }
                EditorGUILayout.EndScrollView();

                EditorGUI.BeginDisabledGroup(_mmdToNonMmdBlendShapeMappings.Count == 0);
                {
                    if (mappingsComponent != null)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        EditorGUILayout.ObjectField(L.Tr("MmdScanAndImportWindow:WillImportToField"), mappingsComponent, typeof(BlendShapeMappings), true);
                        EditorGUI.EndDisabledGroup();

                        var existing = _mmdToNonMmdBlendShapeMappings.Where(mapping => mappingsComponent.HasBlendShapeMappings(mapping.Key));
                        _replaceExisting = EditorGUILayout.Toggle(L.Tr("MmdScanAndImportWindow:ReplaceExistingToggle"), _replaceExisting);

                        bool noNew = !_replaceExisting && existingMappings.Count == _mmdToNonMmdBlendShapeMappings.Count;
                        EditorGUI.BeginDisabledGroup(noNew);
                        if (GUILayout.Button(noNew || (_replaceExisting && existingMappings.Count > 0)
                            ? string.Format(L.Tr("MmdScanAndImportWindow:ImportWithReplacementsButton"), newMappings.Count, existingMappings.Count)
                            : string.Format(L.Tr("MmdScanAndImportWindow:ImportButton"), newMappings.Count)))
                        {
                            ImportTo(mappingsComponent, _mmdToNonMmdBlendShapeMappings, _replaceExisting);
                            EditorGUIUtility.PingObject(mappingsComponent.gameObject);
                        }
                        EditorGUI.EndDisabledGroup();
                    }
                    else
                    {
                        if (_chooseImportDestination)
                        {
                            _customImportDestination = (GameObject)EditorGUILayout.ObjectField(L.Tr("MmdScanAndImportWindow:ImportToField"), _customImportDestination, typeof(GameObject), true);
                        }
                        EditorGUILayout.BeginHorizontal();
                        if (_chooseImportDestination)
                        {
                            EditorGUI.BeginDisabledGroup(_customImportDestination == null);
                            if (GUILayout.Button(L.Tr("MmdScanAndImportWindow:ImportToSelectedGameObjectButton")))
                            {
                                ImportTo(AddMappingsComponentTo(_customImportDestination), _mmdToNonMmdBlendShapeMappings, false);
                                EditorGUIUtility.PingObject(_customImportDestination);
                            }
                            EditorGUI.EndDisabledGroup();
                        }
                        else
                        {
                            if (GUILayout.Button(L.Tr("MmdScanAndImportWindow:ImportToNewBrandedGameObjectButton")))
                            {
                                var importDestination = CreateGameObject(_avatar);
                                ImportTo(AddMappingsComponentTo(importDestination), _mmdToNonMmdBlendShapeMappings, false);
                                EditorGUIUtility.PingObject(importDestination);
                            }
                        }
                        if (GUILayout.Button("▼", EditorStyles.miniButton, GUILayout.Width(20)))
                        {
                            var menu = new GenericMenu();
                            menu.AddItem(new GUIContent(L.Tr("MmdScanAndImportWindow:ChooseDestinationGameObjectToggle")), _chooseImportDestination, () => _chooseImportDestination = !_chooseImportDestination);
                            menu.ShowAsContext();
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }
        }

        private static GameObject CreateGameObject(VRCAvatarDescriptor avatar)
        {
            var childObject = new GameObject { name = "Make It MMD" };
            childObject.transform.parent = avatar.gameObject.transform;
            return childObject;
        }

        private static BlendShapeMappings AddMappingsComponentTo(GameObject gameObject)
        {
            return gameObject.AddComponent<BlendShapeMappings>();
        }

        private static void ImportTo(BlendShapeMappings mappingsComponent, IEnumerable<KeyValuePair<string, string>> imports, bool replaceExisting)
        {
            var mappingsToImport = replaceExisting ? imports : imports.Where(mapping => !mappingsComponent.HasBlendShapeMappings(mapping.Key)).ToList();
            int mappingsImported = 0;
            try
            {
                foreach (var mmdToNonMmdMapping in mappingsToImport)
                {
                    if (mappingsComponent.HasBlendShapeMappings(mmdToNonMmdMapping.Key))
                    {
                        mappingsComponent.DeleteAllBlendShapeMappings(mmdToNonMmdMapping.Key);
                    }
                    mappingsComponent.AddBlendShapeMapping(mmdToNonMmdMapping.Key, mmdToNonMmdMapping.Value);
                    mappingsImported++;
                }
            }
            catch { throw; } // Rely on Unity to catch the exception before continuing to show the import complete dialog.
            finally
            {
                EditorUtility.DisplayDialog(
                    L.Tr("MmdScanAndImportWindow:ImportCompleteDialogTitle"),
                    string.Format(L.Tr("MmdScanAndImportWindow:ImportCompleteDialogMessage"), mappingsImported),
                    L.Tr("MmdScanAndImportWindow:ImportCompleteDialogOKButton"));
            }
        }

        private void Scan()
        {
            var visemeSkinnedMesh = _avatar?.VisemeSkinnedMesh;
            var mesh = visemeSkinnedMesh?.sharedMesh;
            if (mesh == null)
            {
                // TODO: provide a reason for why scan failed.
                return;
            }

            _mmdToNonMmdBlendShapeMappings.Clear();

            // First get the list of MMD and non-MMD blend shapes to reduce the search space.
            // Use List not Dictionary to preserve ordering and always search top-to-bottom.
            _mmdBlendShapes.Clear();
            _nonMmdBlendShapes.Clear();
            for (int i = 0; i < visemeSkinnedMesh?.sharedMesh.blendShapeCount; i++)
            {
                string blendShapeName = visemeSkinnedMesh.sharedMesh.GetBlendShapeName(i);
                // TODO: check all languages
                if (MmdBlendShapeNames.All.Any(blendShape => blendShape.Name == blendShapeName))
                {
                    _mmdBlendShapes.Add((blendShapeName, i));
                }
                else
                {
                    _nonMmdBlendShapes.Add((blendShapeName, i));
                }
            }
            Debug.Log("Found " + _mmdBlendShapes.Count + " MMD blend shapes and " + _nonMmdBlendShapes.Count + " non-MMD blend shapes");
            Debug.Log("Vertex count = " + mesh.vertexCount);
            // Now for each MMD blend shape, scan all non-MMD blend shapes to find which is a match.
            // If there's a lot of blend shapes, this O(mmd * non-mmd * frames) is going to be really slow.
            // (I've only seen f = 1 with sample size of 1, so this may not be much of an issue.. if this sample is representative)
            // Ideas for optimization:
            // - Avoid refetches: Cache these Vector3 if not too big?
            // - Avoid refretches + Speed up comparison: Hash each blend shape and compare these hashes instead?
            // - Speed up comparison: Multithreaded comparison if possible in Unity?
            // (Further reading:
            // https://stackoverflow.com/questions/10534937/comparing-long-strings-by-their-hashes
            // https://stackoverflow.com/questions/12116699/compare-the-contents-of-large-files)
            foreach (var mmdBlendShape in _mmdBlendShapes)
            {
                string mmdBlendShapeName = mmdBlendShape.Item1;
                int mmdBlendShapeIndex = mmdBlendShape.Item2;
                int mmdFrameCount = mesh.GetBlendShapeFrameCount(mmdBlendShapeIndex);
                var mmdDeltaVertices = new Vector3[mesh.vertexCount];
                var mmdDeltaNormals = new Vector3[mesh.vertexCount];
                var mmdDeltaTangents = new Vector3[mesh.vertexCount];
                foreach (var nonMmdBlendShape in _nonMmdBlendShapes)
                {
                    string nonMmdBlendShapeName = nonMmdBlendShape.Item1;
                    int nonMmdBlendShapeIndex = nonMmdBlendShape.Item2;
                    int nonMmdFrameCount = mesh.GetBlendShapeFrameCount(nonMmdBlendShapeIndex);
                    if (nonMmdFrameCount != mmdFrameCount)
                    {
                        continue;
                    }
                    var nonMmdDeltaVertices = new Vector3[mesh.vertexCount];
                    var nonMmdDeltaNormals = new Vector3[mesh.vertexCount];
                    var nonMmdDeltaTangents = new Vector3[mesh.vertexCount];
                    bool matched = true;
                    for (int f = 0; f < mmdFrameCount; f++)
                    {
                        mesh.GetBlendShapeFrameVertices(mmdBlendShapeIndex, f, mmdDeltaVertices, mmdDeltaNormals, mmdDeltaTangents);
                        mesh.GetBlendShapeFrameVertices(nonMmdBlendShapeIndex, f, nonMmdDeltaVertices, nonMmdDeltaNormals, nonMmdDeltaTangents);
                        if (!Enumerable.SequenceEqual(mmdDeltaVertices, nonMmdDeltaVertices)
                            || !Enumerable.SequenceEqual(mmdDeltaNormals, nonMmdDeltaNormals)
                            || !Enumerable.SequenceEqual(mmdDeltaTangents, nonMmdDeltaTangents))
                        {
                            matched = false;
                            break;
                        }
                    }
                    if (matched)
                    {
                        _mmdToNonMmdBlendShapeMappings[mmdBlendShapeName] = nonMmdBlendShapeName;
                        break;
                    }
                }
            }
            Debug.Log("Finished search");
            _doneSearch = true;
        }
    }
}

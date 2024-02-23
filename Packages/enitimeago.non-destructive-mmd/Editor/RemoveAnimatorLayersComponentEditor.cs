using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using enitimeago.NonDestructiveMMD.vendor.d4rkAvatarOptimizer;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    [CustomEditor(typeof(RemoveAnimatorLayersComponent))]
    public class RemoveAnimatorLayersComponentEditor : Editor
    {
        private bool _allowCustomLayers = false;
        private bool _allowUnsafeRemovals = false;
        private PersistentData _persistentData;
        private CommonChecks _commonChecks;

        public void OnEnable()
        {
            _persistentData = CreateInstance<PersistentData>();
            _persistentData._this = this;
            _commonChecks = new CommonChecks(isEditor: true);
        }

        public override void OnInspectorGUI()
        {
            var data = (RemoveAnimatorLayersComponent)target;
            var avatar = data.GetComponentInParent<VRCAvatarDescriptor>();

            EditorGUILayout.BeginHorizontal();
            L.DrawLanguagePicker();
            EditorGUILayout.EndHorizontal();

            // Run asserts, however continue rendering GUI if errors are encountered.
            _commonChecks.RunChecks(avatar);

            var fxController = FindFxAnimatorController(avatar);
            if (!_persistentData._isInitialized)
            {
                _persistentData.customLayersToRemove = data.layersToRemove
                    .Where(layerName => fxController == null || !fxController.layers.Any(l => l.name == layerName))
                    .ToArray();
                _persistentData._isInitialized = true;
            }

            if (fxController != null && fxController.layers.Length > 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(L.Tr("RemoveAnimatorLayersComponentEditor:FXAnimatorControllerLayers"));
                GUILayout.FlexibleSpace();
                var settingsButtonContent = EditorGUIUtility.IconContent("_Popup");
                if (GUILayout.Button(settingsButtonContent))
                {
                    var menu = new GenericMenu();
                    if (_persistentData.customLayersToRemove.Length == 0)
                    {
                        menu.AddItem(new GUIContent(L.Tr("RemoveAnimatorLayersComponentEditor:AllowCustomLayers")), _allowCustomLayers, () => _allowCustomLayers = !_allowCustomLayers);
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent(L.Tr("RemoveAnimatorLayersComponentEditor:AllowCustomLayers")), true);
                    }
                    menu.AddItem(new GUIContent(L.Tr("RemoveAnimatorLayersComponentEditor:AllowUnsafeRemovals")), _allowUnsafeRemovals, () => _allowUnsafeRemovals = !_allowUnsafeRemovals);
                    menu.ShowAsContext();
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.Label(
                    EditorApplication.isPlaying ? L.Tr("RemoveAnimatorLayersComponentEditor:PlayModeLayersExplanation") : L.Tr("RemoveAnimatorLayersComponentEditor:EditModeLayersExplanation"),
                    EditorStyles.wordWrappedLabel);

                EditorGUI.indentLevel++;
                // Disable all edits in play mode, since it will be a clone of not the original FX itself.
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                {
                    var layersAfterRemoval = fxController.layers.Where(layer => !data.layersToRemove.Contains(layer.name)).ToList();
                    var firstThreeRect = EditorGUILayout.BeginVertical();
                    EditorGUI.DrawRect(firstThreeRect, new Color(0.3f, 0.3f, 0.3f, 1.0f));

                    var safeToRemove = UselessFXLayerFinder.FindInAvatar(avatar);
                    for (int i = 0; i < fxController.layers.Length; i++)
                    {
                        var layer = fxController.layers[i];
                        AnimatorControllerLayer nextLayer = null;
                        for (int j = i + 1; j < fxController.layers.Length; j++)
                        {
                            var potentialNextLayer = fxController.layers[i];
                            if (!data.layersToRemove.Contains(potentialNextLayer.name))
                            {
                                nextLayer = potentialNextLayer;
                                break;
                            }
                        }

                        EditorGUILayout.BeginHorizontal();
                        EditorGUI.BeginDisabledGroup(!safeToRemove.Contains(i) && !_allowUnsafeRemovals);
                        bool toggled = data.layersToRemove.Contains(layer.name);
                        bool newToggled = EditorGUILayout.ToggleLeft(layer.name, toggled);
                        EditorGUILayout.LabelField(safeToRemove.Contains(i) ? L.Tr("RemoveAnimatorLayersComponentEditor:SafeToRemove") : L.Tr("RemoveAnimatorLayersComponentEditor:UnsafeToRemove"));
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.EndHorizontal();

                        if (newToggled != toggled)
                        {
                            Undo.RecordObject(data, "Modify layers to remove");
                            if (newToggled)
                            {
                                data.layersToRemove.Add(layer.name);
                            }
                            else
                            {
                                data.layersToRemove.Remove(layer.name);
                            }
                            RebuildModelDataFromViewData();
                        }

                        if (i == 2 || (i < 2 && i == fxController.layers.Length - 1))
                        {
                            EditorGUILayout.EndVertical();
                        }
                    }
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.indentLevel--;
            }

            if (_allowCustomLayers || _persistentData.customLayersToRemove.Length > 0)
            {
                // Disable all edits in play mode, since it will be a clone of not the original FX itself.
                EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
                {
                    var persistentData = new SerializedObject(_persistentData);
                    var customLayersToRemoveProperty = persistentData.FindProperty(nameof(PersistentData.customLayersToRemove));
                    EditorGUILayout.PropertyField(customLayersToRemoveProperty, new GUIContent(L.Tr("RemoveAnimatorLayersComponentEditor:CustomLayersToRemove")));
                    persistentData.ApplyModifiedProperties();
                }
                EditorGUI.EndDisabledGroup();
            }
        }

        private void RebuildModelDataFromViewData()
        {
            var data = (RemoveAnimatorLayersComponent)target;
            var avatar = data.GetComponentInParent<VRCAvatarDescriptor>();
            var fxController = FindFxAnimatorController(avatar);

            // Use OrderedDictionary to preserve known layer order.
            var newLayersToRemove = new OrderedDictionary();

            // First add known layers.
            foreach (string layerToRemove in data.layersToRemove
                .Where(layerToRemove => fxController != null && fxController.layers.Any(l => l.name == layerToRemove) && !newLayersToRemove.Contains(layerToRemove)))
            {
                newLayersToRemove.Add(layerToRemove, true);
            }

            // Then add all unknown layers.
            foreach (string layerToRemove in _persistentData.customLayersToRemove
                .Where(layerToRemove => fxController == null || !fxController.layers.Any(l => l.name == layerToRemove) || !newLayersToRemove.Contains(layerToRemove)))
            {
                newLayersToRemove.Add(layerToRemove, true);
            }

            data.layersToRemove.Clear();
            data.layersToRemove.AddRange(newLayersToRemove.Keys.Cast<string>());
        }

        private AnimatorController FindFxAnimatorController(VRCAvatarDescriptor avatar)
        {
            AnimatorController fxController = null;
            var fxLayersWithWeightControlRef = new HashSet<int>(); // https://creators.vrchat.com/avatars/state-behaviors/#animator-layer-controller
            if (avatar != null)
            {
                foreach (var layer in avatar.baseAnimationLayers)
                {
                    var controller = layer.animatorController as AnimatorController;
                    if (controller == null)
                    {
                        continue;
                    }

                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX && layer.animatorController != null && fxController == null)
                    {
                        fxController = controller;
                    }

                    for (int i = 0; i < controller.layers.Length; i++)
                    {
                        var stateMachine = controller.layers[i].stateMachine;
                        if (stateMachine == null)
                        {
                            continue;
                        }
                        foreach (var behaviour in stateMachine.behaviours.Union(stateMachine.states.SelectMany(s => s.state.behaviours)))
                        {
                            if (behaviour is VRCAnimatorLayerControl layerControl && layerControl.playable == VRC_AnimatorLayerControl.BlendableLayer.FX)
                            {
                                fxLayersWithWeightControlRef.Add(layerControl.layer);
                            }
                        }
                    }
                }
            }
            return fxController;
        }

        // Use this to persist customLayersToRemove, as it's not a 1:1 mapping with the underlying layersToRemove field but we need to persist it past GUI updates.
        // It has to be its own ScriptableObject, because properties on the top-level Editor class are not editable.
        internal class PersistentData : ScriptableObject
        {
            internal RemoveAnimatorLayersComponentEditor _this;
            internal bool _isInitialized = false;
            public string[] customLayersToRemove;

            // This is called on all changes including undo/redo, whereas SerializedObject#ApplyModifiedProperties only catches directly-made edits.
            public void OnValidate()
            {
                if (_this != null)
                {
                    _this.RebuildModelDataFromViewData();
                }
            }
        }
    }
}

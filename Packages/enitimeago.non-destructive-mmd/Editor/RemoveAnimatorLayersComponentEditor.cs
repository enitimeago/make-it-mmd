using UnityEditor;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    [CustomEditor(typeof(RemoveAnimatorLayersComponent))]
    public class RemoveAnimatorLayersComponentEditor : Editor
    {
        private CommonChecks _commonChecks;

        public void OnEnable()
        {
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

            var layersToRemoveProperty = serializedObject.FindProperty(nameof(RemoveAnimatorLayersComponent.layersToRemove));
            EditorGUILayout.PropertyField(layersToRemoveProperty);
        }
    }
}

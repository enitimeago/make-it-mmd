using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    [CustomEditor(typeof(WriteDefaultsComponent))]
    public class WriteDefaultsComponentEditor : Editor
    {
        private CommonChecks _commonChecks;

        public void OnEnable()
        {
            _commonChecks = new CommonChecks(isEditor: true);
        }

        public override void OnInspectorGUI()
        {
            var data = (WriteDefaultsComponent)target;
            var avatar = data.GetComponentInParent<VRCAvatarDescriptor>();

            EditorGUILayout.BeginHorizontal();
            L.DrawLanguagePicker();
            EditorGUILayout.EndHorizontal();

            // Run asserts, however continue rendering GUI if errors are encountered.
            _commonChecks.RunChecks(avatar);

            data.forceAvatarWriteDefaults = EditorGUILayout.Toggle(L.Tr("WriteDefaultsComponentEditor:ForceAvatarWriteDefaultsOn"), data.forceAvatarWriteDefaults);

            GUILayout.Label(L.Tr("WriteDefaultsComponentEditor:ForceAvatarWriteDefaultsOnDescription"), EditorStyles.wordWrappedLabel);
        }
    }
}

#if NDMMD_VRCSDK3_AVATARS

using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    public class CommonChecks
    {
        private bool _isEditor;

        public CommonChecks(bool isEditor)
        {
            _isEditor = isEditor;
        }

        public bool RunChecks(GameObject avatarRootObject)
        {
            var avatarDescriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();
            var mappingsComponents = avatarRootObject.GetComponentsInChildren<BlendShapeMappings>();
            if (mappingsComponents.Length == 0)
            {
                LogLocalized("CommonChecks:NoMMDComponents", Severity.Debug);
                return false;
            }
            if (mappingsComponents.Length > 1)
            {
                LogLocalized("CommonChecks:MultipleMMDComponents", Severity.Error);
                return false;
            }
            return RunChecks(mappingsComponents.First()) && RunChecks(avatarDescriptor);
        }

        public bool RunChecks(BlendShapeMappings blendShapeMappings)
        {
            if (blendShapeMappings.dataVersion > BlendShapeMappings.CURRENT_DATA_VERSION)
            {
                LogLocalized("CommonChecks:NewerDataVersion", Severity.Error);
                return false;
            }
            return true;
        }

        public bool RunChecks(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor == null)
            {
                LogLocalized("CommonChecks:AvatarNotFound", Severity.Warning);
                return false;
            }

            var visemeSkinnedMesh = avatarDescriptor.VisemeSkinnedMesh;
            if (visemeSkinnedMesh == null)
            {
                LogLocalized("CommonChecks:AvatarNoFaceMeshSet", Severity.Warning);
                return false;
            }

            if (visemeSkinnedMesh.name != "Body")
            {
                LogLocalized("CommonChecks:AvatarFaceSMRNotCalledBody", Severity.Warning);
                return false;
            }

            if (visemeSkinnedMesh.sharedMesh == null)
            {
                LogLocalized("CommonChecks:AvatarFaceSMRNoMesh", Severity.Warning);
                return false;
            }

            if (visemeSkinnedMesh.sharedMesh.blendShapeCount == 0)
            {
                LogLocalized("CommonChecks:AvatarFaceSMRNoBlendShapes", Severity.Warning);
                return false;
            }

            for (int i = 0; i < visemeSkinnedMesh.sharedMesh.blendShapeCount; i++)
            {
                string blendShapeName = visemeSkinnedMesh.sharedMesh.GetBlendShapeName(i);
                if (MMDBlendShapes.JapaneseNames().Any(blendShape => blendShape.name == blendShapeName))
                {
                    LogLocalized("CommonChecks:AvatarFaceSMRExistingBlendShapesUnsupported", Severity.Warning);
                    return false;
                }
            }

            return true;
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstackAttribute]
#endif
        private void LogLocalized(string key, Severity severity)
        {
            if (_isEditor)
            {
                string message = L.Tr(key);
                switch (severity)
                {
                    case Severity.Debug: break;
                    case Severity.Warning: EditorGUILayout.HelpBox(message, MessageType.Warning); break;
                    case Severity.Error: EditorGUILayout.HelpBox(message, MessageType.Error); break;
                    default: Debug.LogWarning($"Unknown severity type raised with message \"${message}\""); break;
                }
            }
            else
            {
                switch (severity)
                {
                    case Severity.Debug: Debug.Log(L.Tr(key)); break;
                    case Severity.Warning: ErrorReport.ReportError(L.Localizer, ErrorSeverity.Information, key); break;
                    case Severity.Error: ErrorReport.ReportError(L.Localizer, ErrorSeverity.Error, key); break;
                    default: ErrorReport.ReportError(L.Localizer, ErrorSeverity.InternalError, $"Unknown severity type raised with message \"${key}\""); break;
                }
            }
        }

        // TODO: align with NDMF severity
        public enum Severity
        {
            Debug,
            Warning,
            Error
        }
    }
}

#endif

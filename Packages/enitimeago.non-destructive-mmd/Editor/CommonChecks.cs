#if NDMMD_VRCSDK3_AVATARS

using System.Linq;
using Linguini.Shared.Types.Bundle;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
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

        public bool RunChecks(GameObject avatarRootObject, bool isBuildTime = true)
        {
            if (avatarRootObject == null)
            {
                LogLocalized(Severity.Warning, "CommonChecks:AvatarNotFound");
                return false;
            }
            var avatarDescriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();
            var mappingsComponents = avatarRootObject.GetComponentsInChildren<BlendShapeMappings>();
            var writeDefaultsComponents = avatarRootObject.GetComponentsInChildren<WriteDefaultsComponent>();
            if (mappingsComponents.Length == 0)
            {
                LogLocalized(Severity.Debug, "CommonChecks:NoMMDComponents");
                return false;
            }
            if (mappingsComponents.Length > 1)
            {
                LogLocalized(Severity.Error, "CommonChecks:MultipleMMDComponents");
                return false;
            }
            if (writeDefaultsComponents.Length > 1)
            {
                LogLocalized(Severity.Error, "CommonChecks:MultipleMMDComponents");
                return false;
            }
            if (!RunChecks(mappingsComponents.First()) || !RunChecks(avatarDescriptor, isBuildTime))
            {
                return false;
            }
            var mesh = avatarDescriptor.VisemeSkinnedMesh.sharedMesh;
            bool foundNonExistingReference = false;
            foreach (var mapping in mappingsComponents.First().blendShapeMappings)
            {
                foreach (string avatarKey in mapping.Value.Select(x => x.Key))
                {
                    if (mesh.GetBlendShapeIndex(avatarKey) < 0)
                    {
                        LogLocalized(Severity.Warning, "CommonChecks:MorphReferencesNonExistingBlendShape", ("morphName", (FluentString)mapping.Key), ("blendShapeName", (FluentString)avatarKey));
                        foundNonExistingReference = true;
                    }
                }
            }
            if (foundNonExistingReference)
            {
                return false;
            }
            return true;
        }

        public bool RunChecks(BlendShapeMappings blendShapeMappings)
        {
            if (blendShapeMappings.dataVersion > BlendShapeMappings.CURRENT_DATA_VERSION)
            {
                LogLocalized(Severity.Error, "CommonChecks:NewerDataVersion");
                return false;
            }
            return true;
        }

        // TODO: use an enum or types to decide what checks to skip?
        public bool RunChecks(VRCAvatarDescriptor avatarDescriptor, bool isBuildTime = true)
        {
            if (avatarDescriptor == null)
            {
                LogLocalized(Severity.Warning, "CommonChecks:AvatarNotFound");
                return false;
            }

            var visemeSkinnedMesh = avatarDescriptor.VisemeSkinnedMesh;
            if (visemeSkinnedMesh == null)
            {
                LogLocalized(Severity.Warning, "CommonChecks:AvatarNoFaceMeshSet");
                return false;
            }

            if (visemeSkinnedMesh.sharedMesh == null)
            {
                LogLocalized(Severity.Warning, "CommonChecks:AvatarFaceSMRNoMesh");
                return false;
            }

            if (visemeSkinnedMesh.sharedMesh.blendShapeCount == 0)
            {
                LogLocalized(Severity.Warning, "CommonChecks:AvatarFaceSMRNoBlendShapes");
                return false;
            }

            // TODO: need to introduce ignore for build time too?
            // e.g. if another tool handles these. or maybe just rely on ordering?
            if (isBuildTime)
            {
                if (visemeSkinnedMesh.name != "Body")
                {
                    LogLocalized(Severity.Warning, "CommonChecks:AvatarFaceSMRNotCalledBody");
                }

                if (visemeSkinnedMesh.transform.parent?.gameObject != avatarDescriptor.gameObject)
                {
                    LogLocalized(Severity.Warning, "CommonChecks:AvatarFaceSMRNotAtRoot");
                }

                var writeDefaultsComponents = avatarDescriptor.gameObject.GetComponentsInChildren<WriteDefaultsComponent>();
                if (writeDefaultsComponents.Count() == 0 && AvatarHasWriteDefaultOff(avatarDescriptor))
                {
                    LogLocalized(Severity.Warning, "CommonChecks:AvatarWriteDefaultOffFound");
                }
            }

            return true;
        }

        public bool AvatarHasWriteDefaultOff(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor?.baseAnimationLayers == null)
            {
                return false;
            }
            VRCAvatarDescriptor.CustomAnimLayer fxLayer;
            AnimatorController fxController = null;
            foreach (var layer in avatarDescriptor.baseAnimationLayers)
            {
                if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX && layer.animatorController != null)
                {
                    fxLayer = layer;
                    fxController = layer.animatorController as AnimatorController;
                    break;
                }
            }
            if (fxController != null)
            {
                foreach (var layer in fxController.layers)
                {
                    foreach (var state in layer.stateMachine.states)
                    {
                        if (!state.state.writeDefaultValues)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstackAttribute]
#endif
        private void LogLocalized(Severity severity, string key, params (string, IFluentType)[] args)
        {
            string message = L.Tr(key, key, args);
            if (_isEditor)
            {
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
                // switch (severity)
                // {
                //     case Severity.Debug: Debug.Log(message); break;
                //     case Severity.Warning: ErrorReport.ReportError(L.Localizer, ErrorSeverity.NonFatal, key, args); break;
                //     case Severity.Error: ErrorReport.ReportError(L.Localizer, ErrorSeverity.Error, key, args); break;
                //     default: ErrorReport.ReportError(L.Localizer, ErrorSeverity.InternalError, $"Unknown severity type raised with message \"${key}\""); break;
                // }
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

#if NDMMD_VRCSDK3_AVATARS

using enitimeago.NonDestructiveMMD.vendor;
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
    public class CommonAsserts
    {
        private bool _isEditor;

        public CommonAsserts(bool isEditor)
        {
            _isEditor = isEditor;
        }

        public bool RunAsserts(GameObject avatarRootObject)
        {
            var avatarDescriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();
            return RunAsserts(avatarDescriptor);
        }

        public bool RunAsserts(VRCAvatarDescriptor avatarDescriptor)
        {
            if (avatarDescriptor == null)
            {
                Log("Couldn't find avatar!", Severity.Warning);
                return false;
            }

            var visemeSkinnedMesh = avatarDescriptor.VisemeSkinnedMesh;
            if (visemeSkinnedMesh == null)
            {
                Log("Avatar has no face skin mesh set!", Severity.Warning);
                return false;
            }

            if (visemeSkinnedMesh.name != "Body")
            {
                Log("Avatar face mesh must be called \"Body\"!", Severity.Warning);
                return false;
            }

            for (int i = 0; i < visemeSkinnedMesh.sharedMesh.blendShapeCount; i++)
            {
                string blendShapeName = visemeSkinnedMesh.sharedMesh.GetBlendShapeName(i);
                if (MMDBlendShapes.JapaneseNames().Any(blendShape => blendShape.name == blendShapeName))
                {
                    Log("Avatars with pre-existing MMD blend shapes are unsupported!", Severity.Warning);
                    return false;
                }
            }

            return true;
        }

#if UNITY_2021_3_OR_NEWER
        [HideInCallstackAttribute]
#endif
        private void Log(string message, Severity severity)
        {
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
                switch (severity)
                {
                    case Severity.Debug: Debug.Log(message); break;
                    case Severity.Warning: Debug.LogWarning(message); break;
                    case Severity.Error: Debug.LogError(message); break;
                    default: Debug.LogWarning($"Unknown severity type raised with message \"${message}\""); break;
                }
            }
        }

        public enum Severity
        {
            Debug,
            Warning,
            Error
        }
    }
}

#endif

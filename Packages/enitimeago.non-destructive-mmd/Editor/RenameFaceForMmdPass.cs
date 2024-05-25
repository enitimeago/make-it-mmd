using System.Linq;
using enitimeago.NonDestructiveMMD.vendor;
using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

namespace enitimeago.NonDestructiveMMD
{
    public class RenameFaceForMmdPass : Pass<RenameFaceForMmdPass>
    {
        public override string DisplayName => "Rename Face For MMD";

        protected override void Execute(BuildContext context)
        {
            Execute(context.AvatarRootObject);
        }

        internal void Execute(GameObject avatarRootObject)
        {
            // TODO: need some checks but if we run checks we'll see mmd shape keys exist
            // var commonChecks = new CommonChecks(isEditor: false);
            // if (!commonChecks.RunChecks(avatarRootObject))
            // {
            //     return;
            // }

            var renameFaceForMmdComponent = avatarRootObject.GetComponentInChildren<RenameFaceForMmdComponent>();
            if (renameFaceForMmdComponent == null)
            {
                return;
            }
            var faceObject = renameFaceForMmdComponent.faceObject;
            if (faceObject == null)
            {
                Debug.LogWarning("No face object to rename for MMD, skipping");
                return;
            }

            var skinnedMeshRenderers = avatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            // If "Body" exists on the avatar, it needs to be renamed to something else.
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.name == "Body")
                {
                    skinnedMeshRenderer.name = "Body__ORIGINAL";
                }
            }

            // Rename the face object to "Body".
            faceObject.name = "Body";

            Object.DestroyImmediate(renameFaceForMmdComponent);
        }
    }
}


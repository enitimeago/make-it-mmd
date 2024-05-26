using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using nadena.dev.ndmf;
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
            var descriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();
            if (descriptor.VisemeSkinnedMesh == null)
            {
                Debug.LogWarning("No face object to rename for MMD, skipping");
                return;
            }
            if (descriptor.VisemeSkinnedMesh.name == "Body")
            {
                Debug.LogWarning("Face object is already called Body, skipping");
                return;
            }

            var skinnedMeshRenderers = avatarRootObject.GetComponentsInChildren<SkinnedMeshRenderer>();

            // Objects called "Body" on the avatar need to be renamed to something else.
            // TODO: maybe this only needs to happen to top-level objects. or only Body needs to be top-level.
            var renames = DetermineRenames(skinnedMeshRenderers);
            foreach (var rename in renames)
            {
                rename.skinnedMeshRenderer.name = rename.newName;
            }

            // Rename the face object to "Body".
            descriptor.VisemeSkinnedMesh.name = "Body";

            Object.DestroyImmediate(renameFaceForMmdComponent);
        }

        public struct RenameInfo
        {
            public SkinnedMeshRenderer skinnedMeshRenderer;
            public string newName;
        }

        public static List<RenameInfo> DetermineRenames(IEnumerable<SkinnedMeshRenderer> skinnedMeshRenderers)
        {
            var renames = new List<RenameInfo>();
            int suffixCount = 0;
            var existingNames = skinnedMeshRenderers.Select(smr => smr.name).ToImmutableHashSet();
            foreach (var skinnedMeshRenderer in skinnedMeshRenderers)
            {
                if (skinnedMeshRenderer.name == "Body")
                {
                    string candidateName;
                    do
                    {
                        candidateName = "Body (Original)" + (suffixCount > 0 ? $" ({suffixCount})" : "");
                        suffixCount++;
                    }
                    while (existingNames.Contains(candidateName));
                    renames.Add(new RenameInfo { skinnedMeshRenderer = skinnedMeshRenderer, newName = candidateName });
                }
            }
            return renames;
        }
    }
}


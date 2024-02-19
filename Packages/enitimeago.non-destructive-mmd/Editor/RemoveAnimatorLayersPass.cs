#if NDMMD_VRCSDK3_AVATARS

using System.Linq;
using enitimeago.NonDestructiveMMD.vendor;
using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    public class RemoveAnimatorLayersPass : Pass<RemoveAnimatorLayersPass>
    {
        public override string DisplayName => "Remove FX Animator Layers";

        protected override void Execute(BuildContext context)
        {
            // TODO: dedup this with WriteDefaultsPass
            AnimationUtil.CloneAllControllers(context);
            Execute(context.AvatarRootObject);
        }

        /// <summary>
        /// Pass implementation. Assumes animator controllers are safe to mutate.
        /// ThiS MUST be run after a deep clone, otherwise the avatar's underlying files WILL BE MUTATED.
        /// </summary>
        /// <param name="avatarRootObject"></param>
        internal void Execute(GameObject avatarRootObject)
        {
            // TODO: need some checks but if we run checks we'll see mmd shape keys exist
            // var commonChecks = new CommonChecks(isEditor: false);
            // if (!commonChecks.RunChecks(avatarRootObject))
            // {
            //     return;
            // }

            var removeLayersComponent = avatarRootObject.GetComponentInChildren<RemoveAnimatorLayersComponent>();
            if (removeLayersComponent == null)
            {
                return;
            }
            var layersToRemove = removeLayersComponent.layersToRemove;
            if (layersToRemove.Length == 0)
            {
                return;
            }

            var descriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();

            // Find the FX controller from the avatar.
            // This has already been cloned, so it should be safe to mutate directly.
            VRCAvatarDescriptor.CustomAnimLayer fxLayer;
            AnimatorController fxController = null;
            foreach (var layer in descriptor.baseAnimationLayers)
            {
                if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX && layer.animatorController != null)
                {
                    fxLayer = layer;
                    fxController = layer.animatorController as AnimatorController;
                    break;
                }
            }
            if (fxController == null)
            {
                Debug.LogError("Avatar has no FX controller");
                return;
            }

            // Remove the undesired layer(s).
            fxController.layers = fxController.layers.Where(layer => !layersToRemove.Contains(layer.name)).ToArray();
        }
    }
}

#endif

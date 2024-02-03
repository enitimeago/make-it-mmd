#if NDMMD_VRCSDK3_AVATARS

using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    public class WriteDefaultsPass : Pass<WriteDefaultsPass>
    {
        public override string DisplayName => "Set write defaults";

        protected override void Execute(BuildContext context)
        {
            Execute(context.AvatarRootObject);
        }

        internal void Execute(GameObject avatarRootObject)
        {
            // TODO: need some checks but if we run checks we'll see mmd shape keys exist

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

            // State machine of every FX layer needs to have Write Default ON.
            // TODO: this is probably going to cause unintended consequences for more advanced users. figure out how to mitigate?
            foreach (var layer in fxController.layers)
            {
                foreach (var state in layer.stateMachine.states)
                {
                    state.state.writeDefaultValues = true;
                }
            }
        }
    }
}

#endif

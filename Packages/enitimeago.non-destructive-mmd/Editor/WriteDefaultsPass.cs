#if NDMMD_VRCSDK3_AVATARS

using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
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
            // TODO: need some checks but if we run checks we'll see mmd shape keys exist
            // var commonChecks = new CommonChecks(isEditor: false);
            // if (!commonChecks.RunChecks(context.AvatarRootObject))
            // {
            //     return;
            // }

            var mappingsComponent = context.AvatarRootObject.GetComponentInChildren<WriteDefaultsComponent>();
            if (mappingsComponent == null || !mappingsComponent.forceAvatarWriteDefaults)
            {
                return;
            }

            var animatorServicesContext = context.Extension<AnimatorServicesContext>();
            if (!animatorServicesContext.ControllerContext.Controllers.TryGetValue(VRCAvatarDescriptor.AnimLayerType.FX, out var fxController))
            {
                Debug.LogError("Could not find FX controller in avatar");
                return;
            }

            // State machine of every FX layer needs to have Write Default ON.
            // TODO: this is probably going to cause unintended consequences for more advanced users. figure out how to mitigate?
            foreach (var layer in fxController.Layers)
            {
                foreach (var state in layer.StateMachine.States)
                {
                    state.State.WriteDefaultValues = true;
                }
            }

            Object.DestroyImmediate(mappingsComponent);
        }
    }
}

#endif

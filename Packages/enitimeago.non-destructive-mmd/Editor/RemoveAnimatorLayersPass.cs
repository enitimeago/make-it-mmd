#if NDMMD_VRCSDK3_AVATARS

using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    public class RemoveAnimatorLayersPass : Pass<RemoveAnimatorLayersPass>
    {
        public override string DisplayName => "Remove FX Animator Layers";

        protected override void Execute(BuildContext context)
        {
            // TODO: need some checks but if we run checks we'll see mmd shape keys exist
            // var commonChecks = new CommonChecks(isEditor: false);
            // if (!commonChecks.RunChecks(context.AvatarRootObject))
            // {
            //     return;
            // }

            var removeLayersComponent = context.AvatarRootObject.GetComponentInChildren<RemoveAnimatorLayersComponent>();
            if (removeLayersComponent == null)
            {
                return;
            }
            var layersToRemove = removeLayersComponent.layersToRemove;
            if (layersToRemove.Count == 0)
            {
                return;
            }

            var animatorServicesContext = context.Extension<AnimatorServicesContext>();
            if (!animatorServicesContext.ControllerContext.Controllers.TryGetValue(VRCAvatarDescriptor.AnimLayerType.FX, out var fxController))
            {
                Debug.LogError("Could not find FX controller in avatar");
                return;
            }

            // Remove the undesired layer(s).
            fxController.Layers = fxController.Layers.Where(layer => !layersToRemove.Contains(layer.Name)).ToArray();

            Object.DestroyImmediate(removeLayersComponent);
        }
    }
}

#endif

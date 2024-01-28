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

[assembly: ExportsPlugin(typeof(enitimeago.NonDestructiveMMD.MapperPlugin))]

namespace enitimeago.NonDestructiveMMD
{
    public class MapperPlugin : Plugin<MapperPlugin>
    {
        public override string QualifiedName => "enitimeago.non-destructive-mmd";
        public override string DisplayName => "Non-Destructive MMD";

        protected override void Configure()
        {
            var commonAsserts = new CommonAsserts(isEditor: false);
            Sequence seq = InPhase(BuildPhase.Resolving);
            // Clone animator controllers first to allow safe mutation.
            // Modular Avatar does this, but unless this is moved into somewhere common
            // then it's not ideal to rely on it just to clone all animators.
            seq.Run("Clone animators", AnimationUtil.CloneAllControllers);

            seq = InPhase(BuildPhase.Transforming);
            seq.Run("Create MMD mesh", ctx =>
            {
                // Find the mappings component in the avatar.
                // Intentionally search for all instances to allow it to be placed anywhere,
                // as well as enforcing a restriction of one instance per avatar.
                // TODO: should this not be in the transforming phase?
                var mappingsComponents = ctx.AvatarRootObject.GetComponentsInChildren<BlendShapeMappings>();
                if (mappingsComponents.Length > 1)
                {
                    Debug.LogWarning("More than one Make It MMD component found in avatar. Aborting.");
                    return;
                }
                if (mappingsComponents.Length == 0)
                {
                    Debug.Log("No Make It MMD component found in avatar. Nothing to do");
                    return;
                }
                var mappingsComponent = mappingsComponents.First();

                var descriptor = ctx.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();

                if (!commonAsserts.RunAsserts(ctx.AvatarRootObject))
                {
                    return;
                }

                // Find the avatar's face mesh.
                var faceSkinnedMeshRenderer = descriptor.VisemeSkinnedMesh;
                var mesh = faceSkinnedMeshRenderer.sharedMesh;

                var deltaVertices = new Vector3[mesh.vertexCount];
                var deltaNormals = new Vector3[mesh.vertexCount];
                var deltaTangents = new Vector3[mesh.vertexCount];

                // Duplicate the mesh to allow safe mutation.
                var meshCopy = Object.Instantiate(mesh);

                // Make divider dummy shape key.
                meshCopy.AddBlendShapeFrame("------Make It MMD------", 0, deltaVertices, deltaNormals, deltaTangents);
                faceSkinnedMeshRenderer.sharedMesh = meshCopy;

                // Make shape key copies.
                foreach (var mapping in mappingsComponent.blendShapeMappings)
                {
                    int blendShapeIndex = mesh.GetBlendShapeIndex(mapping.avatarKey);
                    Debug.Log("Create MMD shape key " + mapping.mmdKey + " as copy of " + mapping.avatarKey + " (found " + mapping.avatarKey + " as index " + blendShapeIndex + ")");
                    int frameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);
                    for (int f = 0; f < frameCount; f++)
                    {
                        float weight = mesh.GetBlendShapeFrameWeight(blendShapeIndex, f);
                        mesh.GetBlendShapeFrameVertices(blendShapeIndex, f, deltaVertices, deltaNormals, deltaTangents);
                        meshCopy.AddBlendShapeFrame(mapping.mmdKey, weight, deltaVertices, deltaNormals, deltaTangents);
                    }
                }

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

                Debug.Log("Still alive");
            });
        }
    }
}

#endif

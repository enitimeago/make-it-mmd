using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using nadena.dev.ndmf.fluent;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

[assembly: ExportsPlugin(typeof(enitimeago.NonDestructiveMMD.NonDestructiveMMDPlugin))]

namespace enitimeago.NonDestructiveMMD
{
    public class NonDestructiveMMDPlugin : Plugin<NonDestructiveMMDPlugin>
    {
        public override string QualifiedName => "enitimeago.non-destructive-mmd";
        public override string DisplayName => "Non-Destructive MMD";

        protected override void Configure()
        {
            Sequence seq = InPhase(BuildPhase.Transforming);
            // rely on Modular Avatar cloning all animator controllers to make it safe for us to make destructive edits.
            // it seems like we are already safe, but maybe it was because Modular Avatar happened to run before and we weren't unlucky during testing.
            // TODO: this is really not a great idea, but Modular Avatar's clone utils are also not public and aren't trivial. figure something out.
            seq.AfterPlugin("nadena.dev.modular-avatar")
            .Run("Create MMD mesh", ctx =>
            {
                var descriptor = ctx.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
                var faceSkinnedMeshRenderer = descriptor.VisemeSkinnedMesh;

                var mesh = faceSkinnedMeshRenderer.sharedMesh;
                var deltaVertices = new Vector3[mesh.vertexCount];
                var deltaNormals = new Vector3[mesh.vertexCount];
                var deltaTangents = new Vector3[mesh.vertexCount];

                // TODO: i accidentally modified mesh directly and it seemed to not persist BUT NOT SURE IF THIS IS INTENTIONAL WITH NDMF
                // so if NDMF is intended to allow destructive changes and encapsulate those, then don't bother copying.
                var meshCopy = Object.Instantiate(mesh);

                // Make divider dummy shape key.
                meshCopy.AddBlendShapeFrame("------Non-Destructive MMD------", 0, deltaVertices, deltaNormals, deltaTangents);
                faceSkinnedMeshRenderer.sharedMesh = meshCopy;

                // Make shape key copies.
                var mmdComponent = ctx.AvatarRootObject.GetComponent<NonDestructiveMMD>();
                foreach (var mapping in mmdComponent.blendShapeMappings)
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
                // TODO: avoid potential NPE.
                AnimatorController animatorController = null;
                foreach (var layer in descriptor.baseAnimationLayers)
                {
                    if (layer.type == VRCAvatarDescriptor.AnimLayerType.FX && layer.animatorController != null)
                    {
                        animatorController = layer.animatorController as AnimatorController;
                        break;
                    }
                }

                // State machine of every FX layer needs to have Write Default ON.
                // TODO: avoid potential NPE.
                // TODO: this is probably going to cause unintended consequences for more advanced users. figure out how to mitigate?
                if (animatorController != null)
                {
                    foreach (var layer in animatorController.layers)
                    {
                        foreach (var state in layer.stateMachine.states)
                        {
                            state.state.writeDefaultValues = true;
                        }
                    }
                }

                Debug.Log("Still alive");
                Debug.Log(mmdComponent);
            });
        }
    }
}

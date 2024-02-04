#if NDMMD_VRCSDK3_AVATARS

using nadena.dev.ndmf;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    public class MakeBlendShapesPass : Pass<MakeBlendShapesPass>
    {
        public override string DisplayName => "Make blend shapes";

        protected override void Execute(BuildContext context)
        {
            Execute(context.AvatarRootObject);
        }

        internal void Execute(GameObject avatarRootObject)
        {
            var commonChecks = new CommonChecks(isEditor: false);
            if (!commonChecks.RunChecks(avatarRootObject))
            {
                return;
            }

            var mappingsComponent = avatarRootObject.GetComponentInChildren<BlendShapeMappings>();
            if (mappingsComponent.blendShapeMappings.Count == 0)
            {
                return;
            }

            // If we're here, the avatar is found and valid.
            var descriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();

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
        }
    }
}

#endif

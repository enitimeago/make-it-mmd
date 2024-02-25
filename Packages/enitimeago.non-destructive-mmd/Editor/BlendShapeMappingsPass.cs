#if NDMMD_VRCSDK3_AVATARS

using System.Linq;
using enitimeago.NonDestructiveMMD.vendor.BlendShapeCombiner;
using enitimeago.NonDestructiveMMD.vendor.BlendShapeCombiner.Editor;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    public class BlendShapeMappingsPass : Pass<BlendShapeMappingsPass>
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

            // Run simple copies of single keys.
            foreach (var mapping in mappingsComponent.blendShapeMappings.Where(x => x.avatarKeys.Count() == 1))
            {
                int blendShapeIndex = mesh.GetBlendShapeIndex(mapping.avatarKeys[0]);
                Debug.Log("Create MMD shape key " + mapping.mmdKey + " as copy of " + mapping.avatarKeys[0] + " (found " + mapping.avatarKeys[0] + " as index " + blendShapeIndex + ")");
                int frameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);
                for (int f = 0; f < frameCount; f++)
                {
                    float weight = mesh.GetBlendShapeFrameWeight(blendShapeIndex, f);
                    mesh.GetBlendShapeFrameVertices(blendShapeIndex, f, deltaVertices, deltaNormals, deltaTangents);
                    meshCopy.AddBlendShapeFrame(mapping.mmdKey, weight, deltaVertices, deltaNormals, deltaTangents);
                }
            }

            // Run BlendShapeCombiner on multiple keys.
            var multiMappings = mappingsComponent.blendShapeMappings.Where(x => x.avatarKeys.Count() > 1);
            if (multiMappings.Count() > 0)
            {
                faceSkinnedMeshRenderer.sharedMesh = CombinerImpl.MergeBlendShapes(new BlendShapeCombiner
                {
                    targetRenderer = faceSkinnedMeshRenderer,
                    sourceMesh = meshCopy,
                    newKeys = multiMappings
                        .Select(mapping => new NewKey
                        {
                            name = mapping.mmdKey,
                            sourceKeys = mapping.avatarKeys
                                .Select(avatarKey => new SourceKey { name = avatarKey })
                                .ToArray()
                        })
                        .ToArray()
                });
            }
        }
    }
}

#endif

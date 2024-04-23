#if NDMMD_VRCSDK3_AVATARS

using System.Collections.Generic;
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

            // Figure out if we'll be replacing existing blend shapes.
            // TODO: add tests !!!!!
            var existingBlendShapes = new List<string>();
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                existingBlendShapes.Add(mesh.GetBlendShapeName(i));
            }
            if (existingBlendShapes.Any(blendShape => mappingsComponent.blendShapeMappings.ContainsKey(blendShape)))
            {
                // Looks like we will be replacing existing blend shapes.
                // Because there's no way to delete individual, need to delete all and add back desired blend shapes.
                meshCopy.ClearBlendShapes();
                foreach (string existingBlendShape in existingBlendShapes)
                {
                    // i.e. excluding blend shapes that will be replaced.
                    if (mappingsComponent.HasBlendShapeMappings(existingBlendShape))
                    {
                        continue;
                    }
                    int blendShapeIndex = mesh.GetBlendShapeIndex(existingBlendShape);
                    int frameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);
                    for (int f = 0; f < frameCount; f++)
                    {
                        float weight = mesh.GetBlendShapeFrameWeight(blendShapeIndex, f);
                        mesh.GetBlendShapeFrameVertices(blendShapeIndex, f, deltaVertices, deltaNormals, deltaTangents);
                        meshCopy.AddBlendShapeFrame(existingBlendShape, weight, deltaVertices, deltaNormals, deltaTangents);
                    }
                }
            }

            // Make divider dummy shape key.
            meshCopy.AddBlendShapeFrame("------Make It MMD------", 0, deltaVertices, deltaNormals, deltaTangents);
            faceSkinnedMeshRenderer.sharedMesh = meshCopy;

            // Run simple copies of single keys.
            foreach (var mapping in mappingsComponent.blendShapeMappings.Where(x => x.Value.Count == 1 && x.Value.All(s => s.scale == 1.0f)))
            {
                string blendShapeName = mapping.Value[0].blendShapeName;
                int blendShapeIndex = mesh.GetBlendShapeIndex(blendShapeName);
                Debug.Log("Create MMD shape key " + mapping.Key + " as copy of " + blendShapeName + " (found " + blendShapeName + " as index " + blendShapeIndex + ")");
                int frameCount = mesh.GetBlendShapeFrameCount(blendShapeIndex);
                for (int f = 0; f < frameCount; f++)
                {
                    float weight = mesh.GetBlendShapeFrameWeight(blendShapeIndex, f);
                    mesh.GetBlendShapeFrameVertices(blendShapeIndex, f, deltaVertices, deltaNormals, deltaTangents);
                    meshCopy.AddBlendShapeFrame(mapping.Key, weight, deltaVertices, deltaNormals, deltaTangents);
                }
            }

            // Run BlendShapeCombiner on multiple keys or scaled keys.
            var multiMappings = mappingsComponent.blendShapeMappings.Where(x => x.Value.Count > 1 || x.Value.Any(s => s.scale != 1.0f));
            if (multiMappings.Any())
            {
                faceSkinnedMeshRenderer.sharedMesh = CombinerImpl.MergeBlendShapes(new BlendShapeCombiner
                {
                    targetRenderer = faceSkinnedMeshRenderer,
                    sourceMesh = meshCopy,
                    newKeys = multiMappings
                        .Select(mapping => new NewKey
                        {
                            name = mapping.Key,
                            sourceKeys = mapping.Value
                                .Select(selection => new SourceKey { name = selection.blendShapeName, scale = selection.scale })
                                .ToArray()
                        })
                        .ToArray()
                });
            }
        }
    }
}

#endif

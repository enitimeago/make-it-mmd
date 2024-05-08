﻿#if NDMMD_VRCSDK3_AVATARS

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
            var existingBlendShapes = new List<string>();
            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                existingBlendShapes.Add(mesh.GetBlendShapeName(i));
            }
            if (existingBlendShapes.Any(blendShape => mappingsComponent.blendShapeMappings.Any(x => x.mmdKey == blendShape)))
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
            foreach (var mapping in mappingsComponent.blendShapeMappings.Where(x => x.avatarKeys.Count() == 1 && (x.avatarKeyScaleOverrides == null || x.avatarKeyScaleOverrides.Length == 0)))
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

            // Run BlendShapeCombiner on multiple keys or scaled keys.
            var multiMappings = mappingsComponent.blendShapeMappings.Where(x => x.avatarKeys.Count() > 1 || x.avatarKeyScaleOverrides?.Length > 0);
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
                            sourceKeys = mapping.avatarKeyScaleOverrides?.Length > 0
                                ? mapping.avatarKeys.Zip(mapping.avatarKeyScaleOverrides, (avatarKey, scale) => new SourceKey { name = avatarKey, scale = scale }).ToArray()
                                : mapping.avatarKeys.Select(avatarKey => new SourceKey { name = avatarKey }).ToArray()
                        })
                        .ToArray()
                });
            }

            Object.DestroyImmediate(mappingsComponent);
        }
    }
}

#endif

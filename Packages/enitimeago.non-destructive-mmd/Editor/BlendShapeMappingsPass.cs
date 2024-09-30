#if NDMMD_VRCSDK3_AVATARS

using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using L = enitimeago.NonDestructiveMMD.Localization;

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
            var originalMesh = faceSkinnedMeshRenderer.sharedMesh;

            var deltaVertices = new Vector3[originalMesh.vertexCount];
            var deltaNormals = new Vector3[originalMesh.vertexCount];
            var deltaTangents = new Vector3[originalMesh.vertexCount];

            // Duplicate the mesh to allow safe mutation.
            var meshCopy = Object.Instantiate(originalMesh);

            // Figure out if we'll be replacing existing blend shapes.
            var existingBlendShapes = new List<string>();
            for (int i = 0; i < originalMesh.blendShapeCount; i++)
            {
                existingBlendShapes.Add(originalMesh.GetBlendShapeName(i));
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
                    int blendShapeIndex = originalMesh.GetBlendShapeIndex(existingBlendShape);
                    int frameCount = originalMesh.GetBlendShapeFrameCount(blendShapeIndex);
                    for (int f = 0; f < frameCount; f++)
                    {
                        float weight = originalMesh.GetBlendShapeFrameWeight(blendShapeIndex, f);
                        originalMesh.GetBlendShapeFrameVertices(blendShapeIndex, f, deltaVertices, deltaNormals, deltaTangents);
                        meshCopy.AddBlendShapeFrame(existingBlendShape, weight, deltaVertices, deltaNormals, deltaTangents);
                    }
                }
            }

            // Make divider dummy shape key.
            meshCopy.AddBlendShapeFrame("------Make It MMD------", 0, deltaVertices, deltaNormals, deltaTangents);
            faceSkinnedMeshRenderer.sharedMesh = meshCopy;

            foreach (var mapping in mappingsComponent.blendShapeMappings)
            {
                string destBlendShape = mapping.Key;
                var sourceBlendShapes = mapping.Value;

                // It may be possible for a source blend shape to no longer exist, for example if another tool removes it. Report these to the user.
                // TODO: add test.
                var missingBlendShapes = sourceBlendShapes.Where(sourceBlendShape => originalMesh.GetBlendShapeIndex(sourceBlendShape.Key) < 0);
                if (missingBlendShapes.Any())
                {
                    // ErrorReport.ReportError(L.Localizer, ErrorSeverity.Information, "BlendShapeMappingsPass:MissingSourceBlendShape", destBlendShape, string.Join(",", missingBlendShapes.Select(x => x.Key)), sourceBlendShapes.Count);
                    // ErrorReport.ReportError(L.Localizer, ErrorSeverity.Information, "BlendShapeMappingsPass:SkippingMmdMorph", destBlendShape);
                    continue;
                }

                // Run simple copies of single blend shapes. This allows multiple frames.
                // TODO add test for scaling.
                if (mapping.Value.Count == 1)
                {
                    string sourceBlendShape = sourceBlendShapes.First().Key;
                    int sourceBlendShapeIndex = originalMesh.GetBlendShapeIndex(sourceBlendShape);
                    int sourceBlendShapeFrames = originalMesh.GetBlendShapeFrameCount(sourceBlendShapeIndex);
                    float scale = sourceBlendShapes.First().Value.scale;
                    Debug.Log($"Create MMD shape key {destBlendShape} (scale={scale}) as copy of {sourceBlendShape} (found {sourceBlendShape} as index {sourceBlendShapeIndex})");
                    for (int f = 0; f < sourceBlendShapeFrames; f++)
                    {
                        float weight = originalMesh.GetBlendShapeFrameWeight(sourceBlendShapeIndex, f);
                        originalMesh.GetBlendShapeFrameVertices(sourceBlendShapeIndex, f, deltaVertices, deltaNormals, deltaTangents);
                        if (scale != 1)
                        {
                            for (int v = 0; v < originalMesh.vertexCount; v++)
                            {
                                deltaVertices[v] *= scale;
                                deltaNormals[v] *= scale;
                                deltaTangents[v] *= scale;
                            }
                        }
                        meshCopy.AddBlendShapeFrame(destBlendShape, weight, deltaVertices, deltaNormals, deltaTangents);
                    }
                }
                // Multiple blend shapes have their deltas combined.
                else
                {
                    if (sourceBlendShapes
                        .Select(sourceBlendShape => originalMesh.GetBlendShapeIndex(sourceBlendShape.Key))
                        .Select(originalMesh.GetBlendShapeFrameCount)
                        .Any(frames => frames > 1))
                    {
                        // Do not handle multiple frames for now, as it's not common and interpolation doesn't seem straightforward.
                        // TODO: show error in the UI as well.
                        // ErrorReport.ReportError(L.Localizer, ErrorSeverity.Information, "BlendShapeMappingsPass:CombiningWithMultipleFramesUnsupported");
                        // ErrorReport.ReportError(L.Localizer, ErrorSeverity.Information, "BlendShapeMappingsPass:SkippingMmdMorph", destBlendShape);
                        continue;
                    }

                    Debug.Log($"Create MMD shape key {destBlendShape} as combination of {sourceBlendShapes.Count} blend shapes.");

                    // Accumulated deltas for the single frame.
                    float accumulatedWeight = 0;
                    var accumulatedVertices = new Vector3[originalMesh.vertexCount];
                    var accumulatedNormals = new Vector3[originalMesh.vertexCount];
                    var accumulatedTangents = new Vector3[originalMesh.vertexCount];

                    foreach (var sourceBlendShape in sourceBlendShapes)
                    {
                        int sourceBlendShapeIndex = originalMesh.GetBlendShapeIndex(sourceBlendShape.Key);
                        int sourceBlendShapeFrames = originalMesh.GetBlendShapeFrameCount(sourceBlendShapeIndex);
                        float scale = sourceBlendShape.Value.scale;
                        Debug.Log($"Adding deltas from blend shape {sourceBlendShape.Key} (scale={scale})");
                        accumulatedWeight += originalMesh.GetBlendShapeFrameWeight(sourceBlendShapeIndex, 0);
                        originalMesh.GetBlendShapeFrameVertices(sourceBlendShapeIndex, 0, deltaVertices, deltaNormals, deltaTangents);
                        for (int v = 0; v < originalMesh.vertexCount; v++)
                        {
                            accumulatedVertices[v] += deltaVertices[v] * scale;
                            accumulatedNormals[v] += deltaNormals[v] * scale;
                            accumulatedTangents[v] += deltaTangents[v] * scale;
                        }
                    }

                    float averageWeight = accumulatedWeight / sourceBlendShapes.Count;
                    meshCopy.AddBlendShapeFrame(destBlendShape, averageWeight, accumulatedVertices, accumulatedNormals, accumulatedTangents);
                }
            }

            Object.DestroyImmediate(mappingsComponent);
        }
    }
}

#endif

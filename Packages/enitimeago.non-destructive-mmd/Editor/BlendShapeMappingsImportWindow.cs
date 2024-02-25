using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace enitimeago.NonDestructiveMMD
{
    internal class BlendShapeMappingsImportWindow : EditorWindow
    {
        BlendShapeMappings _dataSource;

        public static void ShowWindow(BlendShapeMappings data)
        {
            var window = GetWindow<BlendShapeMappingsImportWindow>("Import MMD Blend Shapes");
            window._dataSource = data;
        }

        public void OnGUI()
        {
            GUILayout.Label("hello world!");
            if (GUILayout.Button("scan"))
            {
                Scan();
            }
        }

        private void Scan()
        {
            var avatar = _dataSource.GetComponentInParent<VRCAvatarDescriptor>();
            var visemeSkinnedMesh = avatar?.VisemeSkinnedMesh;
            var mesh = visemeSkinnedMesh?.sharedMesh;
            if (mesh == null)
            {
                // TODO: provide a reason for why scan failed.
                return;
            }

            var mmdToNonMmdBlendShapeMappings = new Dictionary<string, string>();

            // First make a list of MMD and non-MMD blend shapes to reduce the search space.
            // Use List not Dictionary to preserve ordering and always search top-to-bottom.
            var mmdBlendShapes = new List<(string, int)>();
            var nonMmdBlendShapes = new List<(string, int)>();
            for (int i = 0; i < visemeSkinnedMesh?.sharedMesh.blendShapeCount; i++)
            {
                string blendShapeName = visemeSkinnedMesh.sharedMesh.GetBlendShapeName(i);
                // TODO: check all languages
                if (MmdBlendShapeNames.All.Any(blendShape => blendShape.Name == blendShapeName))
                {
                    mmdBlendShapes.Add((blendShapeName, i));
                }
                else
                {
                    nonMmdBlendShapes.Add((blendShapeName, i));
                }
            }
            Debug.Log("Found " + mmdBlendShapes.Count + " MMD blend shapes and " + nonMmdBlendShapes.Count + " non-MMD blend shapes");
            Debug.Log("Vertex count = " + mesh.vertexCount);
            // Now for each MMD blend shape, scan all non-MMD blend shapes to find which is a match.
            // If there's a lot of blend shapes, this O(mmd * non-mmd * frames) is going to be really slow.
            // (I've only seen f = 1 with sample size of 1, so this may not be much of an issue.. if this sample is representative)
            // Ideas for optimization:
            // - Avoid refetches: Cache these Vector3 if not too big?
            // - Avoid refretches + Speed up comparison: Hash each blend shape and compare these hashes instead?
            // - Speed up comparison: Multithreaded comparison if possible in Unity?
            // (Further reading:
            // https://stackoverflow.com/questions/10534937/comparing-long-strings-by-their-hashes
            // https://stackoverflow.com/questions/12116699/compare-the-contents-of-large-files)
            foreach (var mmdBlendShape in mmdBlendShapes)
            {
                string mmdBlendShapeName = mmdBlendShape.Item1;
                int mmdBlendShapeIndex = mmdBlendShape.Item2;
                int mmdFrameCount = mesh.GetBlendShapeFrameCount(mmdBlendShapeIndex);
                var mmdDeltaVertices = new Vector3[mesh.vertexCount];
                var mmdDeltaNormals = new Vector3[mesh.vertexCount];
                var mmdDeltaTangents = new Vector3[mesh.vertexCount];
                foreach (var nonMmdBlendShape in nonMmdBlendShapes)
                {
                    string nonMmdBlendShapeName = nonMmdBlendShape.Item1;
                    int nonMmdBlendShapeIndex = nonMmdBlendShape.Item2;
                    int nonMmdFrameCount = mesh.GetBlendShapeFrameCount(nonMmdBlendShapeIndex);
                    if (nonMmdFrameCount != mmdFrameCount)
                    {
                        continue;
                    }
                    var nonMmdDeltaVertices = new Vector3[mesh.vertexCount];
                    var nonMmdDeltaNormals = new Vector3[mesh.vertexCount];
                    var nonMmdDeltaTangents = new Vector3[mesh.vertexCount];
                    bool matched = true;
                    for (int f = 0; f < mmdFrameCount; f++)
                    {
                        mesh.GetBlendShapeFrameVertices(mmdBlendShapeIndex, f, mmdDeltaVertices, mmdDeltaNormals, mmdDeltaTangents);
                        mesh.GetBlendShapeFrameVertices(nonMmdBlendShapeIndex, f, nonMmdDeltaVertices, nonMmdDeltaNormals, nonMmdDeltaTangents);
                        if (!Enumerable.SequenceEqual(mmdDeltaVertices, nonMmdDeltaVertices)
                            || !Enumerable.SequenceEqual(mmdDeltaNormals, nonMmdDeltaNormals)
                            || !Enumerable.SequenceEqual(mmdDeltaTangents, nonMmdDeltaTangents))
                        {
                            matched = false;
                            break;
                        }
                    }
                    if (matched)
                    {
                        mmdToNonMmdBlendShapeMappings[mmdBlendShapeName] = nonMmdBlendShapeName;
                        break;
                    }
                }
            }

            foreach (var entry in mmdToNonMmdBlendShapeMappings)
            {
                Debug.Log("mmd " + entry.Key + " = " + entry.Value);
            }
            Debug.Log("finished search");
        }
    }
}

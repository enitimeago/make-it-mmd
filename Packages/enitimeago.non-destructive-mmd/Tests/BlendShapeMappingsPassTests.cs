using enitimeago.NonDestructiveMMD;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public class BlendShapeMappingsPassTests : TestBase
{
    [Test]
    public void RunPass_WithEmptyMappings_DoesNothing()
    {
        var pass = new BlendShapeMappingsPass();
        var avatar = CreateAvatarWithExpectedFaceName();
        var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
        var mesh = descriptor.VisemeSkinnedMesh.sharedMesh;
        int blendShapeCount = mesh.blendShapeCount;
        var mmdObject = new GameObject();
        mmdObject.transform.parent = avatar.transform;
        mmdObject.AddComponent<BlendShapeMappings>();

        pass.Execute(avatar);

        Assert.AreEqual(descriptor.VisemeSkinnedMesh.sharedMesh, mesh);
        Assert.AreEqual(descriptor.VisemeSkinnedMesh.sharedMesh.blendShapeCount, blendShapeCount);
    }

    [Test]
    public void RunPass_WithMappings_ClonesMesh()
    {
        var pass = new BlendShapeMappingsPass();
        var avatar = CreateAvatarWithExpectedFaceName();
        var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
        var mesh = descriptor.VisemeSkinnedMesh.sharedMesh;
        var mmdObject = new GameObject();
        mmdObject.transform.parent = avatar.transform;
        var mappings = mmdObject.AddComponent<BlendShapeMappings>();
        mappings.blendShapeMappings.Add(new MMDToAvatarBlendShape("あ", new string[] { "vrc.v_aa" }));
        mappings.blendShapeMappings.Add(new MMDToAvatarBlendShape("い", new string[] { "vrc.v_ih" }));
        mappings.blendShapeMappings.Add(new MMDToAvatarBlendShape("う", new string[] { "vrc.v_ou" }));

        pass.Execute(avatar);

        Assert.AreNotEqual(descriptor.VisemeSkinnedMesh.sharedMesh, mesh);
    }

    [Test]
    public void RunPass_WithMappings_AddsMappings()
    {
        var pass = new BlendShapeMappingsPass();
        var avatar = CreateAvatarWithExpectedFaceName();
        var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
        var mesh = descriptor.VisemeSkinnedMesh.sharedMesh;
        int blendShapeCount = mesh.blendShapeCount;
        var mmdObject = new GameObject();
        mmdObject.transform.parent = avatar.transform;
        var mappings = mmdObject.AddComponent<BlendShapeMappings>();
        mappings.blendShapeMappings.Add(new MMDToAvatarBlendShape("あ", new string[] { "vrc.v_aa" }));
        mappings.blendShapeMappings.Add(new MMDToAvatarBlendShape("い", new string[] { "vrc.v_ih" }));
        mappings.blendShapeMappings.Add(new MMDToAvatarBlendShape("う", new string[] { "vrc.v_ou" }));

        pass.Execute(avatar);

        var newMesh = descriptor.VisemeSkinnedMesh.sharedMesh;
        int newBlendShapeCount = newMesh.blendShapeCount;
        Assert.AreEqual(newBlendShapeCount, blendShapeCount + 4);
        Assert.AreEqual(newMesh.GetBlendShapeName(newBlendShapeCount - 4), "------Make It MMD------");
        Assert.AreEqual(newMesh.GetBlendShapeName(newBlendShapeCount - 3), "あ");
        Assert.AreEqual(newMesh.GetBlendShapeName(newBlendShapeCount - 2), "い");
        Assert.AreEqual(newMesh.GetBlendShapeName(newBlendShapeCount - 1), "う");
        AssertBlendShapesEqual(newMesh, "あ", "vrc.v_aa");
        AssertBlendShapesEqual(newMesh, "い", "vrc.v_ih");
        AssertBlendShapesEqual(newMesh, "う", "vrc.v_ou");
    }

    private void AssertBlendShapesEqual(Mesh mesh, string newBlendShapeName, string oldBlendShapeName)
    {
        int newBlendShapeIndex = mesh.GetBlendShapeIndex(newBlendShapeName);
        int oldBlendShapeIndex = mesh.GetBlendShapeIndex(oldBlendShapeName);
        Assert.AreEqual(mesh.GetBlendShapeFrameCount(newBlendShapeIndex), mesh.GetBlendShapeFrameCount(oldBlendShapeIndex));
        for (int f = 0; f < mesh.GetBlendShapeFrameCount(newBlendShapeIndex); f++)
        {
            var newDeltaVertices = new Vector3[mesh.vertexCount];
            var newDeltaNormals = new Vector3[mesh.vertexCount];
            var newDeltaTangents = new Vector3[mesh.vertexCount];
            var oldDeltaVertices = new Vector3[mesh.vertexCount];
            var oldDeltaNormals = new Vector3[mesh.vertexCount];
            var oldDeltaTangents = new Vector3[mesh.vertexCount];
            mesh.GetBlendShapeFrameVertices(newBlendShapeIndex, f, newDeltaVertices, newDeltaNormals, newDeltaTangents);
            mesh.GetBlendShapeFrameVertices(oldBlendShapeIndex, f, oldDeltaVertices, oldDeltaNormals, oldDeltaTangents);
            Assert.AreEqual(mesh.GetBlendShapeFrameWeight(newBlendShapeIndex, f), mesh.GetBlendShapeFrameWeight(oldBlendShapeIndex, f));
            Assert.AreEqual(newDeltaVertices, oldDeltaVertices);
            Assert.AreEqual(newDeltaNormals, oldDeltaNormals);
            Assert.AreEqual(newDeltaTangents, oldDeltaTangents);
        }
    }
}

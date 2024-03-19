using System.Linq;
using enitimeago.NonDestructiveMMD;
using NUnit.Framework;
using UnityEngine;

public class BlendShapeMappingsTests
{
    [Test]
    public void AddBlendShapeMapping_NewMapping_AddsMapping()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();

        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey");

        Assert.IsTrue(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(1, blendShapeMappings.blendShapeMappings.Count);
        Assert.AreEqual("MmdKey", blendShapeMappings.blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new string[] { "AvatarKey" }, blendShapeMappings.blendShapeMappings[0].avatarKeys);
    }

    [Test]
    public void AddBlendShapeMapping_ExistingMapping_AppendsAvatarKey()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();

        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey1");
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey2");

        Assert.IsTrue(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(1, blendShapeMappings.blendShapeMappings.Count);
        Assert.AreEqual("MmdKey", blendShapeMappings.blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new string[] { "AvatarKey1", "AvatarKey2" }, blendShapeMappings.blendShapeMappings[0].avatarKeys);
    }

    [Test]
    public void UpdateBlendShapeMapping_ExistingMapping_UpdatesScale()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey");

        blendShapeMappings.UpdateBlendShapeMapping("MmdKey", "AvatarKey", 0.33f);

        Assert.IsTrue(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(1, blendShapeMappings.blendShapeMappings.Count);
        Assert.AreEqual("MmdKey", blendShapeMappings.blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new string[] { "AvatarKey" }, blendShapeMappings.blendShapeMappings[0].avatarKeys);
        Assert.AreEqual(new float[] { 0.33f }, blendShapeMappings.blendShapeMappings[0].avatarKeyScaleOverrides);
    }

    [Test]
    public void HasBlendShapeMappings_ExistingMapping_ReturnsTrue()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey");

        bool result = blendShapeMappings.HasBlendShapeMappings("MmdKey");

        Assert.IsTrue(result);
    }

    [Test]
    public void HasBlendShapeMappings_NonExistingMapping_ReturnsFalse()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();

        bool result = blendShapeMappings.HasBlendShapeMappings("MmdKey");

        Assert.IsFalse(result);
    }

    [Test]
    public void DeleteAllBlendShapeMappings_ExistingMapping_RemovesMapping()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey");

        blendShapeMappings.DeleteAllBlendShapeMappings("MmdKey");

        Assert.IsFalse(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(0, blendShapeMappings.blendShapeMappings.Count);
    }

    [Test]
    public void DeleteBlendShapeMapping_ExistingMapping_RemovesAvatarKey()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey1");
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey2");

        blendShapeMappings.DeleteBlendShapeMapping("MmdKey", "AvatarKey1");

        Assert.IsTrue(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(1, blendShapeMappings.blendShapeMappings.Count);
        Assert.AreEqual("MmdKey", blendShapeMappings.blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new string[] { "AvatarKey2" }, blendShapeMappings.blendShapeMappings[0].avatarKeys);
    }

    [Test]
    public void OnValidate_MigratesDataVersion0()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();
        blendShapeMappings.dataVersion = 0;
        blendShapeMappings.blendShapeMappings.Add(new MMDToAvatarBlendShape("MmdKey1", new string[] { }) { legacyAvatarKey = "LegacyAvatarKey1" });
        blendShapeMappings.blendShapeMappings.Add(new MMDToAvatarBlendShape("MmdKey2", new string[] { }) { legacyAvatarKey = "LegacyAvatarKey2" });

        blendShapeMappings.OnValidate();

        Assert.AreEqual(1, blendShapeMappings.dataVersion);
        Assert.AreEqual(2, blendShapeMappings.blendShapeMappings.Count);
        var mmdKey1Mapping = blendShapeMappings.blendShapeMappings.FirstOrDefault(x => x.mmdKey == "MmdKey1");
        var mmdKey2Mapping = blendShapeMappings.blendShapeMappings.FirstOrDefault(x => x.mmdKey == "MmdKey2");
        Assert.IsNotNull(mmdKey1Mapping);
        Assert.IsNotNull(mmdKey2Mapping);
        Assert.AreEqual(new string[] { "LegacyAvatarKey1" }, mmdKey1Mapping.avatarKeys);
        Assert.AreEqual(new string[] { "LegacyAvatarKey2" }, mmdKey2Mapping.avatarKeys);
    }
}

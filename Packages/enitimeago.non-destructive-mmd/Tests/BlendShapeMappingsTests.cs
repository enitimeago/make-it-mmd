using System.Collections.Generic;
using System.Linq;
using enitimeago.NonDestructiveMMD;
using NUnit.Framework;
using UnityEngine;

public class BlendShapeMappingsTests
{
    [Test]
    public void OnBeforeSerialize_SerializesInMemoryData()
    {
        var blendShapeMappings = new BlendShapeMappings
        {
            blendShapeMappings = new Dictionary<string, BlendShapeSelections>
            {
                { "MmdKey1", new BlendShapeSelections
                    {
                        { "AvatarKey1", new BlendShapeSelectionOptions { scale = 1.0f } },
                        { "AvatarKey2", new BlendShapeSelectionOptions { scale = 0.5f } }
                    }
                },
                { "MmdKey2", new BlendShapeSelections
                    {
                        { "AvatarKey3", new BlendShapeSelectionOptions { scale = 1.0f } }
                    }
                }
            }
        };

        blendShapeMappings.OnBeforeSerialize();

        Assert.AreEqual(2, blendShapeMappings._blendShapeMappings.Count);
        Assert.AreEqual("MmdKey1", blendShapeMappings._blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new[] { "AvatarKey1", "AvatarKey2" }, blendShapeMappings._blendShapeMappings[0].avatarKeys);
        Assert.AreEqual(new[] { 1.0f, 0.5f }, blendShapeMappings._blendShapeMappings[0].avatarKeyScaleOverrides);
        Assert.AreEqual("MmdKey2", blendShapeMappings._blendShapeMappings[1].mmdKey);
        Assert.AreEqual(new[] { "AvatarKey3" }, blendShapeMappings._blendShapeMappings[1].avatarKeys);
        Assert.IsNull(blendShapeMappings._blendShapeMappings[1].avatarKeyScaleOverrides);
    }

    [Test]
    public void OnAfterDeserialize_DeserializesAtRestData()
    {
        var blendShapeMappings = new BlendShapeMappings
        {
            _blendShapeMappings = new List<MMDToAvatarBlendShape>
            {
                new MMDToAvatarBlendShape("MmdKey1", new[] { "AvatarKey1", "AvatarKey2" }, new[] { 1.0f, 0.5f }),
                new MMDToAvatarBlendShape("MmdKey2", new[] { "AvatarKey3" }, new[] { 1.0f }),
                new MMDToAvatarBlendShape("MmdKey3", new[] { "AvatarKey4" })
            }
        };

        blendShapeMappings.OnAfterDeserialize();

        Assert.AreEqual(3, blendShapeMappings.blendShapeMappings.Count);
        Assert.IsTrue(blendShapeMappings.blendShapeMappings.ContainsKey("MmdKey1"));
        Assert.IsTrue(blendShapeMappings.blendShapeMappings.ContainsKey("MmdKey2"));
        Assert.IsTrue(blendShapeMappings.blendShapeMappings.ContainsKey("MmdKey3"));
        CollectionAssert.AreEquivalent(new BlendShapeSelections { { "AvatarKey1", new BlendShapeSelectionOptions { scale = 1.0f } }, { "AvatarKey2", new BlendShapeSelectionOptions { scale = 0.5f } } }.ToList(), blendShapeMappings.blendShapeMappings["MmdKey1"].ToList());
        CollectionAssert.AreEquivalent(new BlendShapeSelections { { "AvatarKey3", new BlendShapeSelectionOptions { scale = 1.0f } } }.ToList(), blendShapeMappings.blendShapeMappings["MmdKey2"].ToList());
        CollectionAssert.AreEquivalent(new BlendShapeSelections { { "AvatarKey4", new BlendShapeSelectionOptions { scale = 1.0f } } }.ToList(), blendShapeMappings.blendShapeMappings["MmdKey3"].ToList());
    }

    [Test]
    public void AddBlendShapeMapping_NewMapping_AddsMapping()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();

        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey");
        blendShapeMappings.OnBeforeSerialize();

        Assert.IsTrue(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(1, blendShapeMappings._blendShapeMappings.Count);
        Assert.AreEqual("MmdKey", blendShapeMappings._blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new string[] { "AvatarKey" }, blendShapeMappings._blendShapeMappings[0].avatarKeys);
    }

    [Test]
    public void AddBlendShapeMapping_ExistingMapping_AppendsAvatarKey()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();

        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey1");
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey2");
        blendShapeMappings.OnBeforeSerialize();

        Assert.IsTrue(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(1, blendShapeMappings._blendShapeMappings.Count);
        Assert.AreEqual("MmdKey", blendShapeMappings._blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new string[] { "AvatarKey1", "AvatarKey2" }, blendShapeMappings._blendShapeMappings[0].avatarKeys);
    }

    [Test]
    public void UpdateBlendShapeMapping_ExistingMapping_UpdatesScale()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey");

        blendShapeMappings.UpdateBlendShapeMapping("MmdKey", "AvatarKey", 0.33f);
        blendShapeMappings.OnBeforeSerialize();

        Assert.IsTrue(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(1, blendShapeMappings._blendShapeMappings.Count);
        Assert.AreEqual("MmdKey", blendShapeMappings._blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new string[] { "AvatarKey" }, blendShapeMappings._blendShapeMappings[0].avatarKeys);
        Assert.AreEqual(new float[] { 0.33f }, blendShapeMappings._blendShapeMappings[0].avatarKeyScaleOverrides);
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
        Assert.AreEqual(0, blendShapeMappings._blendShapeMappings.Count);
    }

    [Test]
    public void DeleteBlendShapeMapping_ExistingMapping_RemovesAvatarKey()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey1");
        blendShapeMappings.AddBlendShapeMapping("MmdKey", "AvatarKey2");

        blendShapeMappings.DeleteBlendShapeMapping("MmdKey", "AvatarKey1");
        blendShapeMappings.OnBeforeSerialize();

        Assert.IsTrue(blendShapeMappings.HasBlendShapeMappings("MmdKey"));
        Assert.AreEqual(1, blendShapeMappings._blendShapeMappings.Count);
        Assert.AreEqual("MmdKey", blendShapeMappings._blendShapeMappings[0].mmdKey);
        Assert.AreEqual(new string[] { "AvatarKey2" }, blendShapeMappings._blendShapeMappings[0].avatarKeys);
    }

    [Test]
    public void OnValidate_MigratesDataVersion0()
    {
        var blendShapeMappings = new GameObject().AddComponent<BlendShapeMappings>();
        blendShapeMappings.dataVersion = 0;
        blendShapeMappings._blendShapeMappings.Add(new MMDToAvatarBlendShape("MmdKey1", new string[] { }) { legacyAvatarKey = "LegacyAvatarKey1" });
        blendShapeMappings._blendShapeMappings.Add(new MMDToAvatarBlendShape("MmdKey2", new string[] { }) { legacyAvatarKey = "LegacyAvatarKey2" });

        blendShapeMappings.OnValidate();

        Assert.AreEqual(1, blendShapeMappings.dataVersion);
        Assert.AreEqual(2, blendShapeMappings.blendShapeMappings.Count);
        var mmdKey1Mapping = blendShapeMappings._blendShapeMappings.FirstOrDefault(x => x.mmdKey == "MmdKey1");
        var mmdKey2Mapping = blendShapeMappings._blendShapeMappings.FirstOrDefault(x => x.mmdKey == "MmdKey2");
        Assert.IsNotNull(mmdKey1Mapping);
        Assert.IsNotNull(mmdKey2Mapping);
        Assert.AreEqual(new string[] { "LegacyAvatarKey1" }, mmdKey1Mapping.avatarKeys);
        Assert.AreEqual(new string[] { "LegacyAvatarKey2" }, mmdKey2Mapping.avatarKeys);
    }
}

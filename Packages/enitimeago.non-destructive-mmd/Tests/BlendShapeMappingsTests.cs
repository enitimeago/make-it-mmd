using System.Linq;
using enitimeago.NonDestructiveMMD;
using NUnit.Framework;
using UnityEngine;

public class BlendShapeMappingsTests
{
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

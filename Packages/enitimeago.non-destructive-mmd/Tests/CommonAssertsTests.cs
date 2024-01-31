using enitimeago.NonDestructiveMMD;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public class CommonAssertsTests
{
    // TODO: add full coverage
    // TODO: add tests that the correct error message is thrown? need to mock the logger?

    [Test]
    public void RunAsserts_EmptyAvatarRootObject_WillFail()
    {
        var commonAsserts = new CommonAsserts(isEditor: false);
        var avatarRootObject = new GameObject();

        bool result = commonAsserts.RunAsserts(avatarRootObject);

        Assert.IsFalse(result);
    }

    [Test]
    public void RunAsserts_AvatarRootObjectWithMultipleMMDComponents_WillFail()
    {
        var commonAsserts = new CommonAsserts(isEditor: false);
        var avatarRootObject = new GameObject();
        var childObject1 = new GameObject();
        childObject1.AddComponent<BlendShapeMappings>();
        childObject1.transform.parent = avatarRootObject.transform;
        var childObject2 = new GameObject();
        childObject2.AddComponent<BlendShapeMappings>();
        childObject2.transform.parent = avatarRootObject.transform;

        bool result = commonAsserts.RunAsserts(avatarRootObject);

        Assert.IsFalse(result);
    }

    [Test]
    public void RunAsserts_BlendShapeMappingsWithCurrentDataVersion_WillPass()
    {
        var commonAsserts = new CommonAsserts(isEditor: false);
        var blendShapeMappings = new BlendShapeMappings
        {
            dataVersion = BlendShapeMappings.CURRENT_DATA_VERSION
        };

        bool result = commonAsserts.RunAsserts(blendShapeMappings);

        Assert.IsTrue(result);
    }

    [Test]
    public void RunAsserts_BlendShapeMappingsWithNewerDataVersion_WillFail()
    {
        var commonAsserts = new CommonAsserts(isEditor: false);
        var blendShapeMappings = new BlendShapeMappings
        {
            dataVersion = BlendShapeMappings.CURRENT_DATA_VERSION + 1
        };

        bool result = commonAsserts.RunAsserts(blendShapeMappings);

        Assert.IsFalse(result);
    }

    [Test]
    public void RunAsserts_NullVRCAvatarDescriptor_WillFail()
    {
        var commonAsserts = new CommonAsserts(isEditor: false);
        VRCAvatarDescriptor vrcAvatarDescriptor = null;

        bool result = commonAsserts.RunAsserts(vrcAvatarDescriptor);

        Assert.IsFalse(result);
    }
}

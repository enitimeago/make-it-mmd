using enitimeago.NonDestructiveMMD;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public class CommonChecksTests
{
    // TODO: add full coverage
    // TODO: add tests that the correct error message is thrown? need to mock the logger?

    [Test]
    public void RunChecks_EmptyAvatarRootObject_WillFail()
    {
        var commonChecks = new CommonChecks(isEditor: false);
        var avatarRootObject = new GameObject();

        bool result = commonChecks.RunChecks(avatarRootObject);

        Assert.IsFalse(result);
    }

    [Test]
    public void RunChecks_AvatarRootObjectWithMultipleMMDComponents_WillFail()
    {
        var commonChecks = new CommonChecks(isEditor: false);
        var avatarRootObject = new GameObject();
        var childObject1 = new GameObject();
        childObject1.AddComponent<BlendShapeMappings>();
        childObject1.transform.parent = avatarRootObject.transform;
        var childObject2 = new GameObject();
        childObject2.AddComponent<BlendShapeMappings>();
        childObject2.transform.parent = avatarRootObject.transform;

        bool result = commonChecks.RunChecks(avatarRootObject);

        Assert.IsFalse(result);
    }

    [Test]
    public void RunChecks_BlendShapeMappingsWithCurrentDataVersion_WillPass()
    {
        var commonChecks = new CommonChecks(isEditor: false);
        var blendShapeMappings = new BlendShapeMappings
        {
            dataVersion = BlendShapeMappings.CURRENT_DATA_VERSION
        };

        bool result = commonChecks.RunChecks(blendShapeMappings);

        Assert.IsTrue(result);
    }

    [Test]
    public void RunChecks_BlendShapeMappingsWithNewerDataVersion_WillFail()
    {
        var commonChecks = new CommonChecks(isEditor: false);
        var blendShapeMappings = new BlendShapeMappings
        {
            dataVersion = BlendShapeMappings.CURRENT_DATA_VERSION + 1
        };

        bool result = commonChecks.RunChecks(blendShapeMappings);

        Assert.IsFalse(result);
    }

    [Test]
    public void RunChecks_NullVRCAvatarDescriptor_WillFail()
    {
        var commonChecks = new CommonChecks(isEditor: false);
        VRCAvatarDescriptor vrcAvatarDescriptor = null;

        bool result = commonChecks.RunChecks(vrcAvatarDescriptor);

        Assert.IsFalse(result);
    }
}

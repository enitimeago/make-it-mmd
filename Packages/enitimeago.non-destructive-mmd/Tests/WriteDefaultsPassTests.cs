using System.Linq;
using enitimeago.NonDestructiveMMD;
using enitimeago.NonDestructiveMMD.vendor;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public class WriteDefaultsPassTests : TestBase
{
    [Test]
    public void RunPass_NoComponent_DoesNothing()
    {
        var pass = new WriteDefaultsPass();
        var avatar = CreateAvatarWithFaceNameAndFX("Body");
        var buildContext = new BuildContext(avatar, null);
        AnimationUtil.CloneAllControllers(buildContext);

        pass.Execute(avatar);

        // TODO: test no changes to layers.
    }

    [Test]
    public void RunPass_WithBasicAvatar_SetsWriteDefaultOn()
    {
        var pass = new WriteDefaultsPass();
        var avatar = CreateAvatarWithFaceNameAndFX("Body");
        var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
        var buildContext = new BuildContext(avatar, null);
        AnimationUtil.CloneAllControllers(buildContext);
        var newObject = new GameObject();
        newObject.transform.parent = avatar.transform;
        var writeDefaultsComponent = newObject.AddComponent<WriteDefaultsComponent>();
        writeDefaultsComponent.forceAvatarWriteDefaults = true;

        pass.Execute(avatar);

        var fxController = descriptor.baseAnimationLayers
            .First(layer => layer.type == VRCAvatarDescriptor.AnimLayerType.FX)
            .animatorController as AnimatorController;
        fxController.layers.ToList().ForEach(
            layer => layer.stateMachine.states.ToList().ForEach(
                state => Assert.IsTrue(state.state.writeDefaultValues)));
    }
}

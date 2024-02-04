using System.Linq;
using enitimeago.NonDestructiveMMD;
using enitimeago.NonDestructiveMMD.vendor;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;

public class WriteDefaultsPassTests : TestBase
{
    [Test]
    public void RunPass_WithBasicAvatar_SetsWriteDefaultOn()
    {
        var pass = new WriteDefaultsPass();
        var avatar = CreateAvatarWithExpectedFaceNameAndFX();
        var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
        var buildContext = new BuildContext(avatar, null);
        AnimationUtil.CloneAllControllers(buildContext);

        pass.Execute(avatar);

        var fxController = descriptor.baseAnimationLayers
            .First(layer => layer.type == VRCAvatarDescriptor.AnimLayerType.FX)
            .animatorController as AnimatorController;
        fxController.layers.ToList().ForEach(
            layer => layer.stateMachine.states.ToList().ForEach(
                state => Assert.IsTrue(state.state.writeDefaultValues)));
    }
}

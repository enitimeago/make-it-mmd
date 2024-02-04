using enitimeago.NonDestructiveMMD;
using enitimeago.NonDestructiveMMD.vendor;
using nadena.dev.ndmf;
using NUnit.Framework;

public class WriteDefaultsPassTests : TestBase
{
    // TODO: This is just a smoke test for now.
    [Test]
    public void RunPass_WithBasicAvatar()
    {
        var pass = new WriteDefaultsPass();
        var avatar = CreateAvatarWithExpectedFaceNameAndFX();
        var buildContext = new BuildContext(avatar, null);
        AnimationUtil.CloneAllControllers(buildContext);

        pass.Execute(avatar);
    }
}

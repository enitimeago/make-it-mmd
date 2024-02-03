using enitimeago.NonDestructiveMMD;
using NUnit.Framework;

public class MakeBlendShapesPassTests : TestBase
{
    // TODO: This is just a smoke test for now.
    [Test]
    public void RunPlugin_WithBasicAvatar()
    {
        var pass = new MakeBlendShapesPass();
        var avatar = CreateAvatarWithExpectedFaceName();

        pass.Execute(avatar);
    }
}

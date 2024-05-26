using System.Linq;
using enitimeago.NonDestructiveMMD;
using enitimeago.NonDestructiveMMD.vendor;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public class RenameFaceForMmdPassTests : TestBase
{
    [Test]
    public void RunPass_NoComponent_DoesNothing()
    {
        var pass = new RenameFaceForMmdPass();
        var avatar = CreateAvatarWithFaceNameAndFX("Face");
        var buildContext = new BuildContext(avatar, null);
        buildContext.ActivateExtensionContext<AnimationServicesContext>();
        AnimationUtil.CloneAllControllers(buildContext);

        pass.Execute(avatar);

        var faceObject = buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject;
        Assert.AreEqual("Face", faceObject.name);
    }

    [Test]
    public void RunPass_WithFaceObject_RenamesFace()
    {
        var pass = new RenameFaceForMmdPass();
        var avatar = CreateAvatarWithFaceNameAndFX("Face");
        var buildContext = new BuildContext(avatar, null);
        buildContext.ActivateExtensionContext<AnimationServicesContext>();
        AnimationUtil.CloneAllControllers(buildContext);
        var newObject = new GameObject();
        newObject.transform.parent = avatar.transform;
        var removeFaceForMmdComponent = newObject.AddComponent<RenameFaceForMmdComponent>();
        removeFaceForMmdComponent.faceObject = buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject;

        pass.Execute(avatar);

        var faceObject = buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject;
        Assert.AreEqual("Body", faceObject.name);
    }

    [Test]
    public void RunPass_WithFaceAndBodyObjects_RenamesFaceAndBody()
    {
        var pass = new RenameFaceForMmdPass();
        var avatar = CreateAvatarWithFaceNameAndFX("Face");
        var buildContext = new BuildContext(avatar, null);
        buildContext.ActivateExtensionContext<AnimationServicesContext>();
        AnimationUtil.CloneAllControllers(buildContext);
        var newMesh = Object.Instantiate(buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject);
        newMesh.name = "Body";
        newMesh.transform.parent = avatar.transform;
        var newObject = new GameObject();
        newObject.transform.parent = avatar.transform;
        var removeFaceForMmdComponent = newObject.AddComponent<RenameFaceForMmdComponent>();
        removeFaceForMmdComponent.faceObject = buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject;

        pass.Execute(avatar);

        var faceObject = buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject;
        var bodyObject = avatar.GetComponentsInChildren<SkinnedMeshRenderer>().Where(smr => smr.gameObject.name != "Body").First();
        Assert.AreEqual("Body", faceObject.name);
        Assert.AreEqual("Body__ORIGINAL", bodyObject.name);
    }

    [Test]
    public void RunPass_WithFaceObjectAndAnimation_UpdatesAnimationReference()
    {
        var pass = new RenameFaceForMmdPass();
        var clip = new AnimationClip();
        clip.SetCurve("Face", typeof(GameObject), "m_IsActive", AnimationCurve.Constant(0, 1, 0));
        var avatar = CreateAvatarWithFaceNameAndSingleMotion("Face", clip);
        var buildContext = new BuildContext(avatar, null);
        buildContext.ActivateExtensionContext<AnimationServicesContext>();
        var newMesh = Object.Instantiate(buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject);
        newMesh.name = "Body";
        newMesh.transform.parent = avatar.transform;
        var newObject = new GameObject();
        newObject.transform.parent = avatar.transform;
        var removeFaceForMmdComponent = newObject.AddComponent<RenameFaceForMmdComponent>();
        removeFaceForMmdComponent.faceObject = buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject;

        pass.Execute(avatar);
        buildContext.DeactivateExtensionContext<AnimationServicesContext>();

        var modifiedClip = FindMotionFromCreatedAvatar(buildContext) as AnimationClip;
        var bindings = AnimationUtility.GetCurveBindings(modifiedClip);
        Assert.IsFalse(bindings.Any(binding => binding.path == "Face"));
        Assert.IsTrue(bindings.Any(binding => binding.path == "Body"));
    }
}

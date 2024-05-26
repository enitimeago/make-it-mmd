using System.Collections.Generic;
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
        newObject.AddComponent<RenameFaceForMmdComponent>();

        pass.Execute(avatar);

        var faceObject = buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject;
        Assert.AreEqual("Body", faceObject.name);
    }

    [Test]
    public void RunPass_WithFaceAndBodyObjects_RenamesBoth()
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
        newObject.AddComponent<RenameFaceForMmdComponent>();

        pass.Execute(avatar);

        Assert.AreEqual("Body", buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject.name);
        var nonBodyNames = avatar.GetComponentsInChildren<SkinnedMeshRenderer>().Where(smr => smr.gameObject.name != "Body").Select(gameObject => gameObject.name);
        CollectionAssert.AreEquivalent(new List<string> { "Body (Original)" }, nonBodyNames.ToList());
    }

    [Test]
    public void RunPass_WithFaceAndMultipleBodyObjects_RenamesAll()
    {
        var pass = new RenameFaceForMmdPass();
        var avatar = CreateAvatarWithFaceNameAndFX("Face");
        var buildContext = new BuildContext(avatar, null);
        buildContext.ActivateExtensionContext<AnimationServicesContext>();
        AnimationUtil.CloneAllControllers(buildContext);
        var newMesh = Object.Instantiate(buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject);
        newMesh.name = "Body";
        newMesh.transform.parent = avatar.transform;
        var newMesh2 = Object.Instantiate(buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject);
        newMesh2.name = "Body";
        newMesh2.transform.parent = avatar.transform;
        var newObject = new GameObject();
        newObject.transform.parent = avatar.transform;
        newObject.AddComponent<RenameFaceForMmdComponent>();

        pass.Execute(avatar);

        Assert.AreEqual("Body", buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject.name);
        var nonBodyNames = avatar.GetComponentsInChildren<SkinnedMeshRenderer>().Where(smr => smr.gameObject.name != "Body").Select(gameObject => gameObject.name);
        CollectionAssert.AreEquivalent(new List<string> { "Body (Original)", "Body (Original) (1)" }, nonBodyNames.ToList());
    }

    [Test]
    public void RunPass_WithFaceAndConflictingBodyObjects_RenamesAll()
    {
        var pass = new RenameFaceForMmdPass();
        var avatar = CreateAvatarWithFaceNameAndFX("Face");
        var buildContext = new BuildContext(avatar, null);
        buildContext.ActivateExtensionContext<AnimationServicesContext>();
        AnimationUtil.CloneAllControllers(buildContext);
        var newMesh = Object.Instantiate(buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject);
        newMesh.name = "Body";
        newMesh.transform.parent = avatar.transform;
        var newMesh2 = Object.Instantiate(buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject);
        newMesh2.name = "Body";
        newMesh2.transform.parent = avatar.transform;
        var newMesh3 = Object.Instantiate(buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject);
        newMesh3.name = "Body (Original)";
        newMesh3.transform.parent = avatar.transform;
        var newObject = new GameObject();
        newObject.transform.parent = avatar.transform;
        newObject.AddComponent<RenameFaceForMmdComponent>();

        pass.Execute(avatar);

        Assert.AreEqual("Body", buildContext.AvatarDescriptor.VisemeSkinnedMesh.gameObject.name);
        var nonBodyNames = avatar.GetComponentsInChildren<SkinnedMeshRenderer>().Where(smr => smr.gameObject.name != "Body").Select(gameObject => gameObject.name);
        CollectionAssert.AreEquivalent(new List<string> { "Body (Original)", "Body (Original) (1)", "Body (Original) (2)" }, nonBodyNames.ToList());
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
        newObject.AddComponent<RenameFaceForMmdComponent>();

        pass.Execute(avatar);
        buildContext.DeactivateExtensionContext<AnimationServicesContext>();

        var modifiedClip = FindMotionFromCreatedAvatar(buildContext) as AnimationClip;
        var bindings = AnimationUtility.GetCurveBindings(modifiedClip);
        Assert.IsFalse(bindings.Any(binding => binding.path == "Face"));
        Assert.IsTrue(bindings.Any(binding => binding.path == "Body"));
    }
}

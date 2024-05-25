using System.Linq;
using System.Runtime.InteropServices;
using enitimeago.NonDestructiveMMD;
using enitimeago.NonDestructiveMMD.vendor;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public class RemoveFaceForMmdPassTests : TestBase
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

        var faceObject = avatar.GetComponent<VRCAvatarDescriptor>().VisemeSkinnedMesh.gameObject;
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
        removeFaceForMmdComponent.faceObject = avatar.GetComponent<VRCAvatarDescriptor>().VisemeSkinnedMesh.gameObject;

        pass.Execute(avatar);

        var faceObject = avatar.GetComponent<VRCAvatarDescriptor>().VisemeSkinnedMesh.gameObject;
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
        var newMesh = Object.Instantiate(avatar.GetComponent<VRCAvatarDescriptor>().VisemeSkinnedMesh.gameObject);
        newMesh.name = "Body";
        newMesh.transform.parent = avatar.transform;
        var newObject = new GameObject();
        newObject.transform.parent = avatar.transform;
        var removeFaceForMmdComponent = newObject.AddComponent<RenameFaceForMmdComponent>();
        removeFaceForMmdComponent.faceObject = avatar.GetComponent<VRCAvatarDescriptor>().VisemeSkinnedMesh.gameObject;

        pass.Execute(avatar);

        var faceObject = avatar.GetComponent<VRCAvatarDescriptor>().VisemeSkinnedMesh.gameObject;
        var bodyObject = avatar.GetComponentsInChildren<SkinnedMeshRenderer>().Where(smr => smr.gameObject.name != "Body").First();
        Assert.AreEqual("Body", faceObject.name);
        Assert.AreEqual("Body__ORIGINAL", bodyObject.name);
    }

    // TODO: add a test, which in its arrange part adds an animation that references the "Face" gameobject, and in the assert part checks that this animation references "Body" isntead
}

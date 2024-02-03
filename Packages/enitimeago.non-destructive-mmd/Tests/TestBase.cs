using System.Collections.Generic;
using System.Linq;
using enitimeago.NonDestructiveMMD;
using nadena.dev.ndmf;
using nadena.dev.ndmf.ui;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

public class TestBase
{
    private const string CoolBananaFbxGuid = "e8dbc158ea7fccf409d337796c179e11";

    [SetUp]
    public virtual void Setup()
    {
        ErrorReport.Clear();
        ErrorReportWindow.DISABLE_WINDOW = true;
    }

    [TearDown]
    public virtual void Teardown()
    {
        ErrorReportWindow.DISABLE_WINDOW = false;
    }

    protected GameObject CreateAvatar()
    {
        string path = AssetDatabase.GUIDToAssetPath(CoolBananaFbxGuid);
        var avatarRootObject = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));
        var faceMeshRenderer = avatarRootObject.GetComponentInChildren<SkinnedMeshRenderer>();

        var vrcAvatarDescriptor = avatarRootObject.AddComponent<VRCAvatarDescriptor>();
        vrcAvatarDescriptor.VisemeSkinnedMesh = faceMeshRenderer;

        var mmdObject = new GameObject();
        mmdObject.transform.parent = avatarRootObject.transform;
        mmdObject.AddComponent<BlendShapeMappings>();

        return avatarRootObject;
    }

    protected GameObject CreateAvatarWithExpectedFaceName()
    {
        var avatarRootObject = CreateAvatar();
        var vrcAvatarDescriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();
        vrcAvatarDescriptor.VisemeSkinnedMesh.gameObject.name = "Body";
        return avatarRootObject;
    }
}

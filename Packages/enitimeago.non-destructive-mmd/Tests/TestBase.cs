﻿using System.Collections.Generic;
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
    private const string FXGuid = "ce6a5f803b7f200468ad1130d9594a45";

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
        return avatarRootObject;
    }

    protected GameObject CreateAvatarWithFaceName(string faceName)
    {
        var avatarRootObject = CreateAvatar();
        var vrcAvatarDescriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();
        vrcAvatarDescriptor.VisemeSkinnedMesh.gameObject.name = faceName;
        return avatarRootObject;
    }

    protected GameObject CreateAvatarWithFaceNameAndFX(string faceName)
    {
        var avatarRootObject = CreateAvatarWithFaceName(faceName);
        var vrcAvatarDescriptor = avatarRootObject.GetComponent<VRCAvatarDescriptor>();
        string path = AssetDatabase.GUIDToAssetPath(FXGuid);
        var animatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);
        Debug.Log(animatorController);
        var customAnimLayer = new VRCAvatarDescriptor.CustomAnimLayer
        {
            type = VRCAvatarDescriptor.AnimLayerType.FX,
            animatorController = animatorController
        };
        vrcAvatarDescriptor.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[] { customAnimLayer };
        vrcAvatarDescriptor.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];
        return avatarRootObject;
    }
}

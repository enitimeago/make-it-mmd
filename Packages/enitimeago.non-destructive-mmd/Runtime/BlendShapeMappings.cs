﻿using System.Collections.Generic;
using System.Linq;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEngine;

namespace enitimeago.NonDestructiveMMD
{
    [System.Serializable]
    public class MMDToAvatarBlendShape
    {
        public string mmdKey;
        public string avatarKey;

        public MMDToAvatarBlendShape(string mmdKey, string avatarKey)
        {
            this.mmdKey = mmdKey;
            this.avatarKey = avatarKey;
        }
    }

    [AddComponentMenu("Scripts/Make It MMD")]
    [DisallowMultipleComponent]
    public class BlendShapeMappings : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public int dataVersion; // TODO: implement this
        public List<MMDToAvatarBlendShape> blendShapeMappings = new List<MMDToAvatarBlendShape>();

        public void RemoveBlendShapeMapping(string mmdKey)
        {
            blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
        }

        public void SetBlendShapeMapping(string mmdKey, string avatarKey)
        {
            blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
            blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, avatarKey));
        }
    }

}
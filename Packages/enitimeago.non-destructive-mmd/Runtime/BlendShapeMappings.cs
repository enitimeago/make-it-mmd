using System.Collections.Generic;
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
        public const int CURRENT_DATA_VERSION = 0;

        public int dataVersion;
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

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace enitimeago.NonDestructiveMMD
{
    [System.Serializable]
    public class MMDToAvatarBlendShape
    {
        public string mmdKey;
        public string[] avatarKeys;
        [FormerlySerializedAs("avatarKey")] public string legacyAvatarKey;

        public MMDToAvatarBlendShape(string mmdKey, IEnumerable<string> avatarKeys)
        {
            this.mmdKey = mmdKey;
            this.avatarKeys = avatarKeys.ToArray();
        }
    }

    [AddComponentMenu("Scripts/Make It MMD")]
    [DisallowMultipleComponent]
    public class BlendShapeMappings : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public const int CURRENT_DATA_VERSION = 1;

        public int dataVersion;
        public List<MMDToAvatarBlendShape> blendShapeMappings = new List<MMDToAvatarBlendShape>();

        // TODO: add unit test to verify migration
        public void OnValidate()
        {
            if (dataVersion == 0)
            {
                var newMappings = blendShapeMappings
                    .Select(x => new MMDToAvatarBlendShape(x.mmdKey, new string[] { x.legacyAvatarKey }))
                    .ToList();
                blendShapeMappings.Clear();
                blendShapeMappings.AddRange(newMappings);
                dataVersion = 1;
            }
        }

        public void RemoveBlendShapeMapping(string mmdKey)
        {
            blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
        }

        public void SetBlendShapeMapping(string mmdKey, string avatarKey)
        {
            blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
            blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, new string[] { avatarKey }));
        }
    }

}

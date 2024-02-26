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

    [AddComponentMenu("Make It MMD/MIM Make MMD BlendShapes")]
    [DisallowMultipleComponent]
    public class BlendShapeMappings : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public const int CURRENT_DATA_VERSION = 1;

        public int dataVersion;
        public List<MMDToAvatarBlendShape> blendShapeMappings = new List<MMDToAvatarBlendShape>();

        public void OnValidate()
        {
            RunMigrations();
            NormalizeData();
        }

        public void AddBlendShapeMapping(string mmdKey, string avatarKey)
        {
            // This is inefficient, but because the underlying data structure is not a dictionary,
            // always assume it may be possible to have duplicate keys.
            // TODO: use NormalizeData to simplify this function.
            var newMappings = new HashSet<string>();
            if (HasBlendShapeMappings(mmdKey))
            {
                newMappings.UnionWith(blendShapeMappings.Where(x => x.mmdKey == mmdKey).SelectMany(x => x.avatarKeys));
                blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
            }
            newMappings.Add(avatarKey);
            blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, newMappings.ToArray()));
        }

        public bool HasBlendShapeMappings(string mmdKey)
        {
            return blendShapeMappings.Any(x => x.mmdKey == mmdKey && x.avatarKeys.Length > 0);
        }

        public void DeleteAllBlendShapeMappings(string mmdKey)
        {
            blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
        }

        public void DeleteBlendShapeMapping(string mmdKey, string avatarKey)
        {
            NormalizeData();
            var mapping = blendShapeMappings.FirstOrDefault(x => x.mmdKey == mmdKey);
            if (mapping != null && mapping.avatarKeys.Contains(avatarKey))
            {
                mapping.avatarKeys = mapping.avatarKeys.Where(x => x != avatarKey).ToArray();
            }
        }

        // TODO: add unit test to verify migration
        private void RunMigrations()
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

        // TODO: unit test?
        private void NormalizeData()
        {
            var seenMmdKeys = new Dictionary<string, MMDToAvatarBlendShape>();
            // Run off a duplicate of the original list, so it's safe to delete as we go along.
            foreach (var blendShapeMapping in blendShapeMappings.ToList())
            {
                if (!seenMmdKeys.ContainsKey(blendShapeMapping.mmdKey))
                {
                    // Mark this as seen.
                    // TODO: dedup avatar keys.
                    seenMmdKeys[blendShapeMapping.mmdKey] = blendShapeMapping;
                }
                else
                {
                    // First unify both sets.
                    var newMappings = new HashSet<string>(seenMmdKeys[blendShapeMapping.mmdKey].avatarKeys);
                    newMappings.UnionWith(blendShapeMapping.avatarKeys);
                    seenMmdKeys[blendShapeMapping.mmdKey].avatarKeys = newMappings.ToArray();
                    // Then delete this mapping.
                    blendShapeMappings.Remove(blendShapeMapping);
                }
            }
        }
    }

}

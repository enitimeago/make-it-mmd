using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace enitimeago.NonDestructiveMMD
{
    /// <summary>
    /// Serializable representation of a mapping from an MMD morph to blend shape(s), along with optional corresponding scale values, on a mesh.
    /// </summary>
    [Serializable]
    public class MMDToAvatarBlendShape
    {
        public string mmdKey;
        public string[] avatarKeys;
        // TODO: consider creating an AvatarBlendShape class to hold metadata. this is being avoided for now so that dataVersion stays at 1.
        public float[] avatarKeyScaleOverrides;
        [FormerlySerializedAs("avatarKey")] public string legacyAvatarKey;

        public MMDToAvatarBlendShape(string mmdKey, IEnumerable<string> avatarKeys)
        {
            this.mmdKey = mmdKey;
            this.avatarKeys = avatarKeys.ToArray();
        }

        public MMDToAvatarBlendShape(string mmdKey, IEnumerable<string> avatarKeys, IEnumerable<float> avatarKeyScaleOverrides)
        {
            this.mmdKey = mmdKey;
            this.avatarKeys = avatarKeys.ToArray();
            this.avatarKeyScaleOverrides = avatarKeyScaleOverrides.ToArray();
        }
    }

    /// <summary>
    /// In-memory representation of blend shape(s), along with optional corresponding scale values, on a mesh.
    /// </summary>
    public class BlendShapeSelections
    {

    }

    // TODO: transition to ISerializationCallbackReceiver to make this data easier to reason about
    [AddComponentMenu("Make It MMD/MIM Make MMD BlendShapes")]
    [DisallowMultipleComponent]
    public class BlendShapeMappings : MonoBehaviour, ISerializationCallbackReceiver, VRC.SDKBase.IEditorOnly
    {
        public const int CURRENT_DATA_VERSION = 1;

        public int dataVersion;

        [FormerlySerializedAs("blendShapeMappings")]
        [SerializeField]
        internal List<MMDToAvatarBlendShape> _blendShapeMappings = new List<MMDToAvatarBlendShape>();
        public Dictionary<string, MMDToAvatarBlendShape> blendShapeMappings = new Dictionary<string, MMDToAvatarBlendShape>();

        public void OnBeforeSerialize()
        {
            _blendShapeMappings.Clear();

            foreach (var mapping in blendShapeMappings)
            {
                _blendShapeMappings.Add(mapping.Value);
            }
        }

        public void OnValidate()
        {
            RunMigrations();
            NormalizeData();
        }

        public void AddBlendShapeMapping(string mmdKey, string avatarKey)
        {
            // Always assume it might be possible that the underlying data is not comformant.
            // Once cleaned up we can make safer assumptions.
            // TODO: this is also why i want to move to ISerializationCallbackReceiver, it's risky to forget to add this and then cause unexpected things to happen
            NormalizeData();

            if (HasBlendShapeMappings(mmdKey))
            {
                var currentMapping = blendShapeMappings.First(x => x.mmdKey == mmdKey);
                if (currentMapping.avatarKeys.Contains(avatarKey))
                {
                    // Nothing to do here, mapping already exists.
                    return;
                }

                blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
                var newAvatarKeys = new List<string>(currentMapping.avatarKeys) { avatarKey };

                // Depending on whether overrides exist...
                // TODO: again this is why i should probably move to ISerializationCallbackReceiver
                if (currentMapping.avatarKeyScaleOverrides?.Length > 0) // TODO: oh no null checks.. which means avatarKeys could be null too..
                {
                    var newAvatarKeyScaleOverrides = new List<float>(currentMapping.avatarKeyScaleOverrides) { 1.0f };
                    blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, newAvatarKeys, newAvatarKeyScaleOverrides));
                }
                else
                {
                    blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, newAvatarKeys));
                }
            }
            else
            {
                blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, new string[] { avatarKey }));
            }
        }

        // Does nothing if the blend shape isn't set.
        public void UpdateBlendShapeMapping(string mmdKey, string avatarKey, float newScale)
        {
            // TODO: please adopt ISerializationCallbackReceiver the technical debt of writing this class like this makes me really sad
            NormalizeData();

            if (HasBlendShapeMappings(mmdKey))
            {
                var currentMapping = blendShapeMappings.First(x => x.mmdKey == mmdKey);
                if (currentMapping.avatarKeys.Contains(avatarKey))
                {
                    int avatarKeyIndex = Array.IndexOf(currentMapping.avatarKeys, avatarKey);
                    List<float> newScaleOverrides;
                    if (currentMapping.avatarKeyScaleOverrides?.Length > 0)
                    {
                        newScaleOverrides = new List<float>(currentMapping.avatarKeyScaleOverrides);
                        newScaleOverrides[avatarKeyIndex] = newScale;
                    }
                    else
                    {
                        newScaleOverrides = currentMapping.avatarKeys.Select(_ => 1.0f).ToList();
                        newScaleOverrides[avatarKeyIndex] = newScale;
                    }

                    var newMapping = new MMDToAvatarBlendShape(mmdKey, currentMapping.avatarKeys.ToArray(), newScaleOverrides);
                    blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
                    blendShapeMappings.Add(newMapping);
                }
            }
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
                if (mapping.avatarKeys.Length == 1)
                {
                    blendShapeMappings.RemoveAll(x => x.mmdKey == mmdKey);
                    return;
                }

                // TODO: please adopt ISerializationCallbackReceiver this parallel deletion is really ugly
                if (mapping.avatarKeyScaleOverrides != null && mapping.avatarKeyScaleOverrides.Length == mapping.avatarKeys.Length)
                {
                    var newScaleOverrides = new List<float>();
                    for (int i = 0; i < mapping.avatarKeys.Length; i++)
                    {
                        if (mapping.avatarKeys[i] != avatarKey)
                        {
                            newScaleOverrides.Add(mapping.avatarKeyScaleOverrides[i]);
                        }
                    }
                    mapping.avatarKeyScaleOverrides = newScaleOverrides.ToArray();
                }

                mapping.avatarKeys = mapping.avatarKeys.Where(x => x != avatarKey).ToArray();
            }
        }

        // TODO: add unit test to verify migration
        private void RunMigrations()
        {
            if (dataVersion == 0)
            {
                var newMappings = blendShapeMappings
                    .Select(x => new MMDToAvatarBlendShape(x.mmdKey, string.IsNullOrEmpty(x.legacyAvatarKey) ? Array.Empty<string>() : new string[] { x.legacyAvatarKey }))
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
                // Delete 1:0 mappings.
                if (blendShapeMapping.avatarKeys.Length == 0)
                {
                    blendShapeMappings.Remove(blendShapeMapping);
                    continue;
                }

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

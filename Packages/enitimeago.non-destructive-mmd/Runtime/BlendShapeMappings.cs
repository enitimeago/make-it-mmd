using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace enitimeago.NonDestructiveMMD
{
    /// <summary>
    /// At-rest representation of a mapping from an MMD morph to a mesh's blend shape(s), along with optional corresponding scale values.
    /// </summary>
    [Serializable]
    public class MMDToAvatarBlendShape
    {
        public string mmdKey;
        public string[] avatarKeys;
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

    public class BlendShapeSelectionOptions
    {
        public float scale;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            return scale == ((BlendShapeSelectionOptions)obj).scale;
        }

        public override int GetHashCode()
        {
            return scale.GetHashCode();
        }
    }

    /// <summary>
    /// In-memory representation of selections of a mesh's blend shape, along with options for each selection.
    /// </summary>
    public class BlendShapeSelections : Dictionary<string, BlendShapeSelectionOptions>
    {
        public BlendShapeSelections() { }

        /// <summary>
        /// Performs a shallow clone.
        /// </summary>
        public BlendShapeSelections Clone()
        {
            return (BlendShapeSelections)this.ToDictionary(entry => entry.Key, entry => entry.Value);
        }
    }

    /// <summary>
    /// Unity component that holds mappings from MMD keys to a mesh's blend shapes.
    /// </summary>
    [AddComponentMenu("Make It MMD/MIM Make MMD BlendShapes")]
    [DisallowMultipleComponent]
    public class BlendShapeMappings : MonoBehaviour, ISerializationCallbackReceiver, VRC.SDKBase.IEditorOnly
    {
        public const int CURRENT_DATA_VERSION = 1;
        public const string BLEND_SHAPE_MAPPINGS_SERIALIZED_PATH = "_blendShapeMappings";

        public int dataVersion;

        public bool ignoreFaceMeshName = false;
        public bool ignoreFaceMeshNotAtRoot = false;
        public bool ignoreWriteDefaultsOff = false;

        [FormerlySerializedAs("blendShapeMappings")]
        [SerializeField]
        internal List<MMDToAvatarBlendShape> _blendShapeMappings = new List<MMDToAvatarBlendShape>();
        public Dictionary<string, BlendShapeSelections> blendShapeMappings = new Dictionary<string, BlendShapeSelections>();

        public void OnBeforeSerialize()
        {
            _blendShapeMappings.Clear();

            foreach (var (mmdKey, blendShapeSelections) in blendShapeMappings.Select(x => (x.Key, x.Value)))
            {
                var avatarKeys = blendShapeSelections.Keys.ToArray();
                if (blendShapeSelections.Values.Any(s => s.scale != 1.0f)) // TODO: use comparison with epsilon instead?
                {
                    var avatarKeyScaleOverrides = avatarKeys.Select(key => blendShapeSelections[key].scale).ToArray();
                    _blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, avatarKeys, avatarKeyScaleOverrides));
                    continue;
                }
                _blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, avatarKeys));
            }
        }

        public void OnAfterDeserialize()
        {
            blendShapeMappings = new Dictionary<string, BlendShapeSelections>();

            foreach (var mapping in _blendShapeMappings)
            {
                var blendShapeSelections = new BlendShapeSelections();

                for (int i = 0; i < mapping.avatarKeys.Length; i++)
                {
                    string avatarKey = mapping.avatarKeys[i];
                    // TODO: handle if avatarKey is seen more than once
                    // TODO: show error to user when reading malformed data so they have a chance to recover
                    blendShapeSelections[avatarKey] = new BlendShapeSelectionOptions
                    {
                        scale = mapping.avatarKeyScaleOverrides != null && i < mapping.avatarKeyScaleOverrides.Length
                            ? mapping.avatarKeyScaleOverrides[i] : 1.0f
                    };
                }

                blendShapeMappings[mapping.mmdKey] = blendShapeSelections;
            }
        }

        public void OnValidate()
        {
            RunMigrations();
        }

        public bool HasBlendShapeMappings(string mmdKey)
        {
            return blendShapeMappings.ContainsKey(mmdKey);
        }

        public bool HasBlendShapeMappings(string mmdKey, out BlendShapeSelections blendShapeSelections)
        {
            return blendShapeMappings.TryGetValue(mmdKey, out blendShapeSelections);
        }

        /// <summary>
        /// Adds a mapping from an MMD key to a blend shape on the avatar. If such a mapping already exists, leaves it unmodified.
        /// </summary>
        public void AddBlendShapeMapping(string mmdKey, string avatarKey)
        {
            if (HasBlendShapeMappings(mmdKey, out var selections) && !selections.ContainsKey(avatarKey))
            {
                selections[avatarKey] = new BlendShapeSelectionOptions { scale = 1.0f };
            }
            else
            {
                blendShapeMappings[mmdKey] = new BlendShapeSelections
                {
                    { avatarKey, new BlendShapeSelectionOptions { scale = 1.0f } }
                };
            }
        }

        /// <summary>
        /// Updates the scale for an existing mapping. Does nothing if the mapping doesn't exist.
        /// </summary>
        public void UpdateBlendShapeMapping(string mmdKey, string avatarKey, float newScale)
        {
            if (HasBlendShapeMappings(mmdKey, out var selections))
            {
                if (selections.TryGetValue(avatarKey, out var selection))
                {
                    selection.scale = newScale;
                }
            }
        }

        public void DeleteAllBlendShapeMappings(string mmdKey)
        {
            blendShapeMappings.Remove(mmdKey);
        }

        public void DeleteBlendShapeMapping(string mmdKey, string avatarKey)
        {
            if (HasBlendShapeMappings(mmdKey, out var selections))
            {
                if (selections.ContainsKey(avatarKey))
                {
                    selections.Remove(avatarKey);
                }
                if (selections.Count == 0)
                {
                    blendShapeMappings.Remove(mmdKey);
                }
            }
        }

        private void RunMigrations()
        {
            if (dataVersion == 0)
            {
                var newMappings = _blendShapeMappings
                    .Select(x => new MMDToAvatarBlendShape(x.mmdKey, string.IsNullOrEmpty(x.legacyAvatarKey) ? Array.Empty<string>() : new string[] { x.legacyAvatarKey }))
                    .ToList();
                _blendShapeMappings.Clear();
                _blendShapeMappings.AddRange(newMappings);
                dataVersion = 1;
                OnAfterDeserialize();
            }
        }
    }

}

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
    /// In-memory representation of a selection of a mesh's blend shape, along with a scale value.
    /// </summary>
    public class BlendShapeSelection
    {
        public string blendShapeName;
        public float scale;
    }

    /// <summary>
    /// Unity component that holds mappings from MMD keys to a mesh's blend shapes.
    /// </summary>
    [AddComponentMenu("Make It MMD/MIM Make MMD BlendShapes")]
    [DisallowMultipleComponent]
    public class BlendShapeMappings : MonoBehaviour, ISerializationCallbackReceiver, VRC.SDKBase.IEditorOnly
    {
        public const int CURRENT_DATA_VERSION = 1;

        public int dataVersion;

        [FormerlySerializedAs("blendShapeMappings")]
        [SerializeField]
        internal List<MMDToAvatarBlendShape> _blendShapeMappings = new List<MMDToAvatarBlendShape>();
        public Dictionary<string, List<BlendShapeSelection>> blendShapeMappings = new Dictionary<string, List<BlendShapeSelection>>();

        public void OnBeforeSerialize()
        {
            _blendShapeMappings.Clear();

            foreach (var (mmdKey, selectionsForMmdKey) in blendShapeMappings.Select(x => (x.Key, x.Value)))
            {
                var avatarKeys = selectionsForMmdKey.Select(s => s.blendShapeName).ToArray();
                if (selectionsForMmdKey.Any(s => s.scale != 1.0f)) // TODO: use comparison with epsilon instead?
                {
                    var avatarKeyScaleOverrides = selectionsForMmdKey.Select(s => s.scale).ToArray();
                    _blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, avatarKeys, avatarKeyScaleOverrides));
                    continue;
                }
                _blendShapeMappings.Add(new MMDToAvatarBlendShape(mmdKey, avatarKeys));
            }
        }

        public void OnAfterDeserialize()
        {
            blendShapeMappings = new Dictionary<string, List<BlendShapeSelection>>();

            foreach (var mapping in _blendShapeMappings)
            {
                var blendShapeSelections = new List<BlendShapeSelection>();

                for (int i = 0; i < mapping.avatarKeys.Length; i++)
                {
                    blendShapeSelections.Add(new BlendShapeSelection
                    {
                        blendShapeName = mapping.avatarKeys[i],
                        scale = mapping.avatarKeyScaleOverrides != null && i < mapping.avatarKeyScaleOverrides.Length
                            ? mapping.avatarKeyScaleOverrides[i] : 1.0f
                    });
                }

                blendShapeMappings[mapping.mmdKey] = blendShapeSelections;
            }
        }

        public void OnValidate()
        {
            RunMigrations();
            NormalizeData();
        }

        public bool HasBlendShapeMappings(string mmdKey)
        {
            return blendShapeMappings.ContainsKey(mmdKey);
        }

        public bool HasBlendShapeMappings(string mmdKey, out List<BlendShapeSelection> blendShapeSelections)
        {
            return blendShapeMappings.TryGetValue(mmdKey, out blendShapeSelections);
        }

        /// <summary>
        /// Adds a mapping from an MMD key to a blend shape on the avatar. If such a mapping already exists, leaves it unmodified.
        /// </summary>
        public void AddBlendShapeMapping(string mmdKey, string avatarKey)
        {
            // Always assume it might be possible that the underlying data is not comformant.
            // TODO: Clean up the in-memory representation so this isn't necessary.
            NormalizeData();

            if (HasBlendShapeMappings(mmdKey, out var selections) && !selections.Any(s => s.blendShapeName == avatarKey))
            {
                selections.Add(new BlendShapeSelection { blendShapeName = avatarKey, scale = 1.0f });
            }
            else
            {
                blendShapeMappings[mmdKey] = new List<BlendShapeSelection>
                {
                    new BlendShapeSelection { blendShapeName = avatarKey, scale = 1.0f }
                };
            }
        }

        /// <summary>
        /// Updates the scale for an existing mapping. Does nothing if the mapping doesn't exist.
        /// </summary>
        public void UpdateBlendShapeMapping(string mmdKey, string avatarKey, float newScale)
        {
            // TODO: Clean up the in-memory representation so this isn't necessary.
            NormalizeData();

            if (HasBlendShapeMappings(mmdKey, out var selections))
            {
                // TODO: Clean up the in-memory representation so this isn't necessary.
                var selection = selections.FirstOrDefault(s => s.blendShapeName == avatarKey);
                if (selection != null)
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
            // TODO: Clean up the in-memory representation so this isn't necessary.
            NormalizeData();

            if (HasBlendShapeMappings(mmdKey, out var selections))
            {
                var selection = selections.FirstOrDefault(s => s.blendShapeName == avatarKey);
                if (selection != null)
                {
                    selections.Remove(selection);
                }
                if (selections.Count == 0)
                {
                    blendShapeMappings.Remove(mmdKey);
                }
            }
        }

        // TODO: add unit test to verify migration
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

        // TODO: unit test?
        private void NormalizeData()
        {
            // TODO: delete 1:0 mappings, duplicate blend shape mappings
        }
    }

}

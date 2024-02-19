using UnityEngine;

namespace enitimeago.NonDestructiveMMD
{
    [AddComponentMenu("Make It MMD/MIM Avatar Write Defaults")]
    [DisallowMultipleComponent]
    public class WriteDefaultsComponent : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public const int CURRENT_DATA_VERSION = 0;

        public int dataVersion;

        public bool forceAvatarWriteDefaults = false;
    }
}

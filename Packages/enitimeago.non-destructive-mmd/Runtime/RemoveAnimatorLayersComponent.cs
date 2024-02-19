using UnityEngine;

namespace enitimeago.NonDestructiveMMD
{
    [AddComponentMenu("Make It MMD/Remove FX Animator Layers")]
    [DisallowMultipleComponent]
    public class RemoveAnimatorLayersComponent : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public const int CURRENT_DATA_VERSION = 0;

        public int dataVersion;

        public string[] layersToRemove;
    }
}

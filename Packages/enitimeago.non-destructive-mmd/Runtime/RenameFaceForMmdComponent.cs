using UnityEngine;

namespace enitimeago.NonDestructiveMMD
{
    [AddComponentMenu("Make It MMD/MIM Rename Face For MMD")]
    [DisallowMultipleComponent]
    public class RenameFaceForMmdComponent : MonoBehaviour, VRC.SDKBase.IEditorOnly
    {
        public const int CURRENT_DATA_VERSION = 0;

        public int dataVersion;

        public GameObject faceObject;
    }
}

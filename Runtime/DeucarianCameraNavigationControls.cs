using UnityEngine;

namespace Deucarian.CameraNavigation
{
    [CreateAssetMenu(
        fileName = "DeucarianCameraNavigationControls",
        menuName = "Deucarian/Camera Navigation/Navigation Controls")]
    public sealed class DeucarianCameraNavigationControls : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float globalSensitivity = 1f;
        [SerializeField, Min(0.01f)] private float orbitRotationSensitivity = 0.45f;
        [SerializeField, Min(0.01f)] private float orbitPanSensitivity = 1.4f;
        [SerializeField, Min(0.01f)] private float orbitZoomSensitivity = 1f;
        [SerializeField, Min(0.01f)] private float flyLookSensitivity = 1.35f;
        [SerializeField, Min(0.01f)] private float flyMoveSensitivity = 1f;
        [SerializeField, Min(1f)] private float boostScale = 4f;

        public float GlobalSensitivity
        {
            get => Mathf.Max(0.01f, globalSensitivity);
            set => globalSensitivity = Mathf.Max(0.01f, value);
        }

        public float OrbitRotationSensitivity => GlobalSensitivity * Mathf.Max(0.01f, orbitRotationSensitivity);
        public float OrbitPanSensitivity => GlobalSensitivity * Mathf.Max(0.01f, orbitPanSensitivity);
        public float OrbitZoomSensitivity => GlobalSensitivity * Mathf.Max(0.01f, orbitZoomSensitivity);
        public float FlyLookSensitivity => GlobalSensitivity * Mathf.Max(0.01f, flyLookSensitivity);
        public float FlyMoveSensitivity => GlobalSensitivity * Mathf.Max(0.01f, flyMoveSensitivity);
        public float BoostScale => Mathf.Max(1f, boostScale);
    }
}

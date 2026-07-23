using UnityEngine;

namespace Deucarian.CameraNavigation
{
    [CreateAssetMenu(
        fileName = "DeucarianCameraNavigationControls",
        menuName = "Deucarian/Camera Navigation/Navigation Controls")]
    public sealed class DeucarianCameraNavigationControls :
        ScriptableObject,
        IDeucarianCameraNavigationControls
    {
        public const float DefaultWheelZoomStep = 0.12f;
        public const float DefaultOrbitMinimumDistance = 0.0001f;
        public const float DefaultOrbitMinimumDistanceScale = 0.00001f;
        public const float DefaultOrbitNearClipDistanceMultiplier = 1.1f;

        [Header("Global")]
        [SerializeField, Min(0.01f)] private float globalSensitivity = 1f;

        [Header("Orbit")]
        [SerializeField, Min(0.01f)] private float orbitRotationSensitivity = 0.45f;
        [SerializeField, Min(0.01f)] private float orbitPanSensitivity = 1.4f;
        [SerializeField, Min(0.01f)] private float orbitZoomSensitivity = 1f;
        [SerializeField] private bool allowInfiniteVerticalOrbit;
        [SerializeField] private bool invertOrbitRotation;
        [Tooltip("Absolute world-space safety floor for perspective Orbit distance.")]
        [SerializeField, Min(0.000001f)] private float orbitMinimumDistance =
            DefaultOrbitMinimumDistance;
        [Tooltip("Minimum distance as a fraction of the current reference bounds radius.")]
        [SerializeField, Min(0f)] private float orbitMinimumDistanceScale =
            DefaultOrbitMinimumDistanceScale;
        [Tooltip("Keeps the Orbit pivot beyond the perspective camera's near clip plane.")]
        [SerializeField, Min(1f)] private float orbitNearClipDistanceMultiplier =
            DefaultOrbitNearClipDistanceMultiplier;

        [Header("Fly")]
        [SerializeField, Min(0.01f)] private float flyLookSensitivity = 1.35f;
        [SerializeField, Min(0.01f)] private float flyMoveSensitivity = 1f;
        [SerializeField, Min(0.01f)] private float flyZoomSensitivity = 1f;

        [Header("Wheel Zoom")]
        [SerializeField, Min(0.0001f)] private float wheelZoomStep = DefaultWheelZoomStep;
        [SerializeField, Range(0.01f, 1f)] private float wheelZoomSmoothingTime = 0.08f;
        [SerializeField, Min(0.0001f)] private float wheelZoomStopEpsilon = 0.001f;

        [Header("Modifiers")]
        [SerializeField, Min(1f)] private float boostScale = 4f;

        public float GlobalSensitivity
        {
            get => Mathf.Max(0.01f, globalSensitivity);
            set => globalSensitivity = Mathf.Max(0.01f, value);
        }

        public float OrbitRotationSensitivity =>
            GlobalSensitivity * Mathf.Max(0.01f, orbitRotationSensitivity);
        public float OrbitPanSensitivity =>
            GlobalSensitivity * Mathf.Max(0.01f, orbitPanSensitivity);
        public float OrbitZoomSensitivity =>
            GlobalSensitivity * Mathf.Max(0.01f, orbitZoomSensitivity);
        public bool AllowInfiniteVerticalOrbit => allowInfiniteVerticalOrbit;
        public bool InvertOrbitRotation => invertOrbitRotation;
        public float OrbitMinimumDistance => Mathf.Max(0.000001f, orbitMinimumDistance);
        public float OrbitMinimumDistanceScale => Mathf.Max(0f, orbitMinimumDistanceScale);
        public float OrbitNearClipDistanceMultiplier =>
            Mathf.Max(1f, orbitNearClipDistanceMultiplier);
        public float FlyLookSensitivity =>
            GlobalSensitivity * Mathf.Max(0.01f, flyLookSensitivity);
        public float FlyMoveSensitivity =>
            GlobalSensitivity * Mathf.Max(0.01f, flyMoveSensitivity);
        public float FlyZoomSensitivity =>
            GlobalSensitivity * Mathf.Max(0.01f, flyZoomSensitivity);
        public float WheelZoomStep => Mathf.Max(0.0001f, wheelZoomStep);
        public float WheelZoomSmoothingTime => Mathf.Max(0.01f, wheelZoomSmoothingTime);
        public float WheelZoomStopEpsilon => Mathf.Max(0.0001f, wheelZoomStopEpsilon);
        public float BoostScale => Mathf.Max(1f, boostScale);

        public static DeucarianCameraNavigationControls CreateRuntimeDefault()
        {
            DeucarianCameraNavigationControls controls =
                CreateInstance<DeucarianCameraNavigationControls>();
            controls.name = "Runtime Deucarian Camera Navigation Controls";
            return controls;
        }

        private void OnValidate()
        {
            globalSensitivity = Mathf.Max(0.01f, globalSensitivity);
            orbitRotationSensitivity = Mathf.Max(0.01f, orbitRotationSensitivity);
            orbitPanSensitivity = Mathf.Max(0.01f, orbitPanSensitivity);
            orbitZoomSensitivity = Mathf.Max(0.01f, orbitZoomSensitivity);
            orbitMinimumDistance = Mathf.Max(0.000001f, orbitMinimumDistance);
            orbitMinimumDistanceScale = Mathf.Max(0f, orbitMinimumDistanceScale);
            orbitNearClipDistanceMultiplier = Mathf.Max(1f, orbitNearClipDistanceMultiplier);
            flyLookSensitivity = Mathf.Max(0.01f, flyLookSensitivity);
            flyMoveSensitivity = Mathf.Max(0.01f, flyMoveSensitivity);
            flyZoomSensitivity = Mathf.Max(0.01f, flyZoomSensitivity);
            wheelZoomStep = Mathf.Max(0.0001f, wheelZoomStep);
            wheelZoomSmoothingTime = Mathf.Max(0.01f, wheelZoomSmoothingTime);
            wheelZoomStopEpsilon = Mathf.Max(0.0001f, wheelZoomStopEpsilon);
            boostScale = Mathf.Max(1f, boostScale);
        }
    }
}

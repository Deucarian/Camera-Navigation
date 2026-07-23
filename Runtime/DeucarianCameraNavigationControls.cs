using UnityEngine;

namespace Deucarian.CameraNavigation
{
    [CreateAssetMenu(
        fileName = "DeucarianCameraNavigationControls",
        menuName = "Deucarian/Camera Navigation/Navigation Controls")]
    public sealed class DeucarianCameraNavigationControls :
        ScriptableObject,
        IDeucarianCameraNavigationControls,
        IDeucarianOrbitNavigationSpeeds,
        IDeucarianFlyNavigationSpeeds
    {
        public const float DefaultGlobalSensitivity = 1f;
        public const float DefaultWheelZoomStep = 0.12f;
        public const float DefaultWheelZoomSmoothingTime = 0.08f;
        public const float DefaultWheelZoomStopEpsilon = 0.001f;
        public const float DefaultBoostScale = 4f;
        public const float DefaultOrbitKeyboardPanSpeed = 0.9f;
        public const float DefaultOrbitMousePanSpeed = 0.0025f;
        public const float DefaultOrbitOrthographicMousePanSpeed = 0.003f;
        public const float DefaultOrbitRotationSpeed = 0.25f;
        public const float DefaultOrbitRotationSensitivity = 10f;
        public const float DefaultOrbitPanSensitivity = 10f;
        public const float DefaultOrbitZoomSensitivity = 1f;
        public const float DefaultOrbitMinimumDistance = 0.0001f;
        public const float DefaultOrbitMinimumDistanceScale = 0.00001f;
        public const float DefaultOrbitNearClipDistanceMultiplier = 1.1f;
        public const float DefaultFlyMoveSpeed = 8f;
        public const float DefaultFlyRotationSpeed = 0.18f;
        public const float DefaultFlyLookSensitivity = 10f;
        public const float DefaultFlyMoveSensitivity = 1f;
        public const float DefaultFlyZoomSensitivity = 1f;

        [Header("Global")]
        [SerializeField, Min(0.01f)] private float globalSensitivity =
            DefaultGlobalSensitivity;

        [Header("Orbit")]
        [SerializeField, Min(0f)] private float orbitKeyboardPanSpeed =
            DefaultOrbitKeyboardPanSpeed;
        [SerializeField, Min(0f)] private float orbitMousePanSpeed =
            DefaultOrbitMousePanSpeed;
        [SerializeField, Min(0f)] private float orbitOrthographicMousePanSpeed =
            DefaultOrbitOrthographicMousePanSpeed;
        [SerializeField, Min(0f)] private float orbitRotationSpeed =
            DefaultOrbitRotationSpeed;
        [SerializeField, Min(0.01f)] private float orbitRotationSensitivity =
            DefaultOrbitRotationSensitivity;
        [SerializeField, Min(0.01f)] private float orbitPanSensitivity =
            DefaultOrbitPanSensitivity;
        [SerializeField, Min(0.01f)] private float orbitZoomSensitivity =
            DefaultOrbitZoomSensitivity;
        [SerializeField] private bool allowInfiniteVerticalOrbit;
        [SerializeField] private bool invertOrbitRotation = true;
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
        [SerializeField, Min(0f)] private float flyMoveSpeed = DefaultFlyMoveSpeed;
        [SerializeField, Min(0f)] private float flyRotationSpeed =
            DefaultFlyRotationSpeed;
        [SerializeField, Min(0.01f)] private float flyLookSensitivity =
            DefaultFlyLookSensitivity;
        [SerializeField, Min(0.01f)] private float flyMoveSensitivity =
            DefaultFlyMoveSensitivity;
        [SerializeField, Min(0.01f)] private float flyZoomSensitivity =
            DefaultFlyZoomSensitivity;

        [Header("Wheel Zoom")]
        [SerializeField, Min(0.0001f)] private float wheelZoomStep = DefaultWheelZoomStep;
        [SerializeField, Range(0.01f, 1f)] private float wheelZoomSmoothingTime =
            DefaultWheelZoomSmoothingTime;
        [SerializeField, Min(0.0001f)] private float wheelZoomStopEpsilon =
            DefaultWheelZoomStopEpsilon;

        [Header("Modifiers")]
        [SerializeField, Min(1f)] private float boostScale = DefaultBoostScale;

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
        public float OrbitKeyboardPanSpeed => Mathf.Max(0f, orbitKeyboardPanSpeed);
        public float OrbitMousePanSpeed => Mathf.Max(0f, orbitMousePanSpeed);
        public float OrbitOrthographicMousePanSpeed =>
            Mathf.Max(0f, orbitOrthographicMousePanSpeed);
        public float OrbitRotationSpeed => Mathf.Max(0f, orbitRotationSpeed);
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
        public float FlyMoveSpeed => Mathf.Max(0f, flyMoveSpeed);
        public float FlyRotationSpeed => Mathf.Max(0f, flyRotationSpeed);
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

        public void ResetToDefaults()
        {
            globalSensitivity = DefaultGlobalSensitivity;
            orbitKeyboardPanSpeed = DefaultOrbitKeyboardPanSpeed;
            orbitMousePanSpeed = DefaultOrbitMousePanSpeed;
            orbitOrthographicMousePanSpeed =
                DefaultOrbitOrthographicMousePanSpeed;
            orbitRotationSpeed = DefaultOrbitRotationSpeed;
            orbitRotationSensitivity = DefaultOrbitRotationSensitivity;
            orbitPanSensitivity = DefaultOrbitPanSensitivity;
            orbitZoomSensitivity = DefaultOrbitZoomSensitivity;
            allowInfiniteVerticalOrbit = false;
            invertOrbitRotation = true;
            orbitMinimumDistance = DefaultOrbitMinimumDistance;
            orbitMinimumDistanceScale = DefaultOrbitMinimumDistanceScale;
            orbitNearClipDistanceMultiplier =
                DefaultOrbitNearClipDistanceMultiplier;
            flyMoveSpeed = DefaultFlyMoveSpeed;
            flyRotationSpeed = DefaultFlyRotationSpeed;
            flyLookSensitivity = DefaultFlyLookSensitivity;
            flyMoveSensitivity = DefaultFlyMoveSensitivity;
            flyZoomSensitivity = DefaultFlyZoomSensitivity;
            wheelZoomStep = DefaultWheelZoomStep;
            wheelZoomSmoothingTime = DefaultWheelZoomSmoothingTime;
            wheelZoomStopEpsilon = DefaultWheelZoomStopEpsilon;
            boostScale = DefaultBoostScale;
        }

        private void OnValidate()
        {
            globalSensitivity = Mathf.Max(0.01f, globalSensitivity);
            orbitKeyboardPanSpeed = Mathf.Max(0f, orbitKeyboardPanSpeed);
            orbitMousePanSpeed = Mathf.Max(0f, orbitMousePanSpeed);
            orbitOrthographicMousePanSpeed =
                Mathf.Max(0f, orbitOrthographicMousePanSpeed);
            orbitRotationSpeed = Mathf.Max(0f, orbitRotationSpeed);
            orbitRotationSensitivity = Mathf.Max(0.01f, orbitRotationSensitivity);
            orbitPanSensitivity = Mathf.Max(0.01f, orbitPanSensitivity);
            orbitZoomSensitivity = Mathf.Max(0.01f, orbitZoomSensitivity);
            orbitMinimumDistance = Mathf.Max(0.000001f, orbitMinimumDistance);
            orbitMinimumDistanceScale = Mathf.Max(0f, orbitMinimumDistanceScale);
            orbitNearClipDistanceMultiplier = Mathf.Max(1f, orbitNearClipDistanceMultiplier);
            flyMoveSpeed = Mathf.Max(0f, flyMoveSpeed);
            flyRotationSpeed = Mathf.Max(0f, flyRotationSpeed);
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

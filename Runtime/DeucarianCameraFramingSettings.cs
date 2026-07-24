using UnityEngine;

namespace Deucarian.CameraNavigation
{
    [CreateAssetMenu(
        fileName = "DeucarianCameraFramingSettings",
        menuName = "Deucarian/Camera Navigation/Camera Framing Settings")]
    public sealed class DeucarianCameraFramingSettings :
        ScriptableObject,
        IDeucarianCameraFramingSettings
    {
        public const string CanonicalResourcesPath =
            "Deucarian/CameraFramingSettings";
        public const float DefaultPaddingMultiplier = 1f;
        public const float DefaultNearClipClearanceMultiplier = 1.05f;

        [Tooltip(
            "Use an orientation supplied by the framing target, or retain the " +
            "camera's current orientation and only pan/zoom.")]
        [SerializeField] private DeucarianCameraFramingRotationPolicy
            rotationPolicy =
                DeucarianCameraFramingRotationPolicy
                    .UsePreferredTargetRotation;

        [Tooltip(
            "Multiplies target-specific framing padding. Values below one " +
            "are clamped so visible bounds cannot be cropped.")]
        [SerializeField, Min(1f)] private float paddingMultiplier =
            DefaultPaddingMultiplier;

        [Tooltip(
            "Keeps the closest framed point beyond the camera's near clip " +
            "plane by this multiplier.")]
        [SerializeField, Min(1f)] private float nearClipClearanceMultiplier =
            DefaultNearClipClearanceMultiplier;

        public DeucarianCameraFramingRotationPolicy RotationPolicy
        {
            get => rotationPolicy;
            set => rotationPolicy = value;
        }

        public float PaddingMultiplier
        {
            get => Mathf.Max(1f, paddingMultiplier);
            set => paddingMultiplier = Mathf.Max(1f, value);
        }

        public float NearClipClearanceMultiplier
        {
            get => Mathf.Max(1f, nearClipClearanceMultiplier);
            set => nearClipClearanceMultiplier = Mathf.Max(1f, value);
        }

        public static DeucarianCameraFramingSettings CreateRuntimeDefault()
        {
            DeucarianCameraFramingSettings settings =
                CreateInstance<DeucarianCameraFramingSettings>();
            settings.name = "Runtime Deucarian Camera Framing Settings";
            return settings;
        }

        public void ResetToDefaults()
        {
            rotationPolicy =
                DeucarianCameraFramingRotationPolicy
                    .UsePreferredTargetRotation;
            paddingMultiplier = DefaultPaddingMultiplier;
            nearClipClearanceMultiplier =
                DefaultNearClipClearanceMultiplier;
        }

        private void OnValidate()
        {
            paddingMultiplier = Mathf.Max(1f, paddingMultiplier);
            nearClipClearanceMultiplier =
                Mathf.Max(1f, nearClipClearanceMultiplier);
        }
    }
}

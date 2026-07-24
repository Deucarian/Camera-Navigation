using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public readonly struct DeucarianCameraFramingTarget
    {
        public const float DefaultPadding = 1.25f;

        public DeucarianCameraFramingTarget(
            Bounds bounds,
            Vector3 focusPoint,
            float padding = DefaultPadding,
            DeucarianCameraFramingDistanceProfile distanceProfile =
                DeucarianCameraFramingDistanceProfile.Standard)
            : this(
                bounds,
                focusPoint,
                false,
                Quaternion.identity,
                padding,
                distanceProfile)
        {
        }

        public DeucarianCameraFramingTarget(
            Bounds bounds,
            Vector3 focusPoint,
            Quaternion preferredCameraRotation,
            float padding = DefaultPadding,
            DeucarianCameraFramingDistanceProfile distanceProfile =
                DeucarianCameraFramingDistanceProfile.Standard)
            : this(
                bounds,
                focusPoint,
                true,
                preferredCameraRotation,
                padding,
                distanceProfile)
        {
        }

        private DeucarianCameraFramingTarget(
            Bounds bounds,
            Vector3 focusPoint,
            bool hasPreferredCameraRotation,
            Quaternion preferredCameraRotation,
            float padding,
            DeucarianCameraFramingDistanceProfile distanceProfile)
        {
            Bounds = bounds;
            FocusPoint = focusPoint;
            HasPreferredCameraRotation = hasPreferredCameraRotation;
            PreferredCameraRotation = preferredCameraRotation;
            Padding = padding;
            DistanceProfile = distanceProfile;
        }

        public Bounds Bounds { get; }
        public Vector3 FocusPoint { get; }
        public bool HasPreferredCameraRotation { get; }
        public Quaternion PreferredCameraRotation { get; }
        public float Padding { get; }
        public DeucarianCameraFramingDistanceProfile DistanceProfile
        {
            get;
        }
    }
}

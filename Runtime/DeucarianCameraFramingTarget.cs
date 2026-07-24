using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public readonly struct DeucarianCameraFramingTarget
    {
        public DeucarianCameraFramingTarget(
            Bounds bounds,
            Vector3 focusPoint,
            float padding = 1.25f)
            : this(
                bounds,
                focusPoint,
                false,
                Quaternion.identity,
                padding)
        {
        }

        public DeucarianCameraFramingTarget(
            Bounds bounds,
            Vector3 focusPoint,
            Quaternion preferredCameraRotation,
            float padding = 1.25f)
            : this(
                bounds,
                focusPoint,
                true,
                preferredCameraRotation,
                padding)
        {
        }

        private DeucarianCameraFramingTarget(
            Bounds bounds,
            Vector3 focusPoint,
            bool hasPreferredCameraRotation,
            Quaternion preferredCameraRotation,
            float padding)
        {
            Bounds = bounds;
            FocusPoint = focusPoint;
            HasPreferredCameraRotation = hasPreferredCameraRotation;
            PreferredCameraRotation = preferredCameraRotation;
            Padding = padding;
        }

        public Bounds Bounds { get; }
        public Vector3 FocusPoint { get; }
        public bool HasPreferredCameraRotation { get; }
        public Quaternion PreferredCameraRotation { get; }
        public float Padding { get; }
    }
}

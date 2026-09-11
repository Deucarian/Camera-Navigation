using System;

namespace Deucarian.CameraNavigation
{
    /// <summary>Central framing configuration. Callers only pass its typed key and target bounds.</summary>
    public sealed class CameraPresetDefinition
    {
        public CameraPresetDefinition(CameraPresetKey key, IDeucarianCameraFramingSettings framing = null,
            float padding = DeucarianCameraFramingTarget.DefaultPadding,
            DeucarianCameraFramingDistanceProfile distanceProfile = DeucarianCameraFramingDistanceProfile.Standard,
            bool animate = true)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            if (padding < 1 || float.IsNaN(padding) || float.IsInfinity(padding)) throw new ArgumentOutOfRangeException(nameof(padding), "Camera padding must be a finite value of at least one.");
            Framing = framing;
            Padding = padding;
            DistanceProfile = distanceProfile;
            Animate = animate;
        }
        public CameraPresetKey Key { get; }
        public IDeucarianCameraFramingSettings Framing { get; }
        public float Padding { get; }
        public DeucarianCameraFramingDistanceProfile DistanceProfile { get; }
        public bool Animate { get; }
    }
}

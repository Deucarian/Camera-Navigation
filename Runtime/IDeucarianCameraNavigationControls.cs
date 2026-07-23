namespace Deucarian.CameraNavigation
{
    public interface IDeucarianCameraNavigationControls
    {
        float OrbitRotationSensitivity { get; }
        float OrbitPanSensitivity { get; }
        float OrbitZoomSensitivity { get; }
        bool AllowInfiniteVerticalOrbit { get; }
        bool InvertOrbitRotation { get; }
        float OrbitMinimumDistance { get; }
        float OrbitMinimumDistanceScale { get; }
        float OrbitNearClipDistanceMultiplier { get; }
        float FlyLookSensitivity { get; }
        float FlyMoveSensitivity { get; }
        float FlyZoomSensitivity { get; }
        float WheelZoomStep { get; }
        float WheelZoomSmoothingTime { get; }
        float WheelZoomStopEpsilon { get; }
        float BoostScale { get; }
    }
}

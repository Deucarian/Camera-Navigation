namespace Deucarian.CameraNavigation
{
    public interface IDeucarianCameraFramingSettings
    {
        DeucarianCameraFramingRotationPolicy RotationPolicy { get; }
        float PaddingMultiplier { get; }
        float RelaxedDistanceMultiplier { get; }
        float NearClipClearanceMultiplier { get; }
    }
}

namespace Deucarian.CameraNavigation
{
    public interface IDeucarianCameraFramingSettings
    {
        DeucarianCameraFramingRotationPolicy RotationPolicy { get; }
        float PaddingMultiplier { get; }
        float NearClipClearanceMultiplier { get; }
    }
}

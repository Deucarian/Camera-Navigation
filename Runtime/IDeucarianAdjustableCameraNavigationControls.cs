namespace Deucarian.CameraNavigation
{
    public interface IDeucarianAdjustableCameraNavigationControls :
        IDeucarianCameraNavigationControls
    {
        float GlobalSensitivity { get; set; }
    }
}

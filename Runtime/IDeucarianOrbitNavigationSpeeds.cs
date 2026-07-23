namespace Deucarian.CameraNavigation
{
    public interface IDeucarianOrbitNavigationSpeeds
    {
        float OrbitKeyboardPanSpeed { get; }
        float OrbitMousePanSpeed { get; }
        float OrbitOrthographicMousePanSpeed { get; }
        float OrbitRotationSpeed { get; }
    }
}

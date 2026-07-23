namespace Deucarian.CameraNavigation
{
    internal static class DeucarianCameraNavigationSpeedResolver
    {
        internal static float GetOrbitKeyboardPanSpeed(
            IDeucarianCameraNavigationControls controls)
        {
            return controls is IDeucarianOrbitNavigationSpeeds speeds
                ? speeds.OrbitKeyboardPanSpeed
                : DeucarianCameraNavigationControls.DefaultOrbitKeyboardPanSpeed;
        }

        internal static float GetOrbitMousePanSpeed(
            IDeucarianCameraNavigationControls controls)
        {
            return controls is IDeucarianOrbitNavigationSpeeds speeds
                ? speeds.OrbitMousePanSpeed
                : DeucarianCameraNavigationControls.DefaultOrbitMousePanSpeed;
        }

        internal static float GetOrbitOrthographicMousePanSpeed(
            IDeucarianCameraNavigationControls controls)
        {
            return controls is IDeucarianOrbitNavigationSpeeds speeds
                ? speeds.OrbitOrthographicMousePanSpeed
                : DeucarianCameraNavigationControls
                    .DefaultOrbitOrthographicMousePanSpeed;
        }

        internal static float GetOrbitRotationSpeed(
            IDeucarianCameraNavigationControls controls)
        {
            return controls is IDeucarianOrbitNavigationSpeeds speeds
                ? speeds.OrbitRotationSpeed
                : DeucarianCameraNavigationControls.DefaultOrbitRotationSpeed;
        }

        internal static float GetFlyMoveSpeed(
            IDeucarianCameraNavigationControls controls)
        {
            return controls is IDeucarianFlyNavigationSpeeds speeds
                ? speeds.FlyMoveSpeed
                : DeucarianCameraNavigationControls.DefaultFlyMoveSpeed;
        }

        internal static float GetFlyRotationSpeed(
            IDeucarianCameraNavigationControls controls)
        {
            return controls is IDeucarianFlyNavigationSpeeds speeds
                ? speeds.FlyRotationSpeed
                : DeucarianCameraNavigationControls.DefaultFlyRotationSpeed;
        }
    }
}

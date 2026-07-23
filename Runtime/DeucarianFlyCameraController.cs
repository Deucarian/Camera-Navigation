using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public sealed class DeucarianFlyCameraController
    {
        private const float MinimumWheelZoomReferenceDistance = 0.25f;

        private readonly DeucarianWheelZoomSmoother wheelZoom =
            new DeucarianWheelZoomSmoother();

        public void SyncZoomState()
        {
            wheelZoom.Reset(0f);
        }

        public void Apply(
            Camera camera,
            DeucarianFlyCameraInput input,
            float deltaTime,
            IDeucarianCameraNavigationControls controls)
        {
            Apply(
                camera,
                input,
                deltaTime,
                controls,
                MinimumWheelZoomReferenceDistance);
        }

        public void Apply(
            Camera camera,
            DeucarianFlyCameraInput input,
            float deltaTime,
            IDeucarianCameraNavigationControls controls,
            float wheelZoomReferenceDistance)
        {
            if (camera == null)
            {
                return;
            }

            if (!input.HasInput &&
                !wheelZoom.HasPending(wheelZoom.Current, GetZoomStopEpsilon(controls)))
            {
                return;
            }

            if (input.HasInput)
            {
                ApplyRotation(camera, input.Look, controls);
                ApplyMovement(camera, input, deltaTime, controls);
                ApplyZoomInput(input.Zoom, wheelZoomReferenceDistance, controls);
            }

            ApplyZoomSmoothing(camera, deltaTime, controls);
        }

        private void ApplyMovement(
            Camera camera,
            DeucarianFlyCameraInput input,
            float deltaTime,
            IDeucarianCameraNavigationControls controls)
        {
            if (input.Move.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 movement =
                camera.transform.right * input.Move.x +
                camera.transform.up * input.Move.y +
                camera.transform.forward * input.Move.z;
            float moveSensitivity =
                controls != null ? controls.FlyMoveSensitivity : 1f;
            camera.transform.position +=
                movement *
                (DeucarianCameraNavigationSpeedResolver.GetFlyMoveSpeed(controls) *
                 moveSensitivity *
                 GetSpeedFactor(input.Boost, input.Slow, controls) *
                 Mathf.Max(0f, deltaTime));
        }

        private void ApplyRotation(
            Camera camera,
            Vector2 lookDelta,
            IDeucarianCameraNavigationControls controls)
        {
            if (!IsFinite(lookDelta.x) ||
                !IsFinite(lookDelta.y) ||
                lookDelta.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float lookSensitivity =
                controls != null ? controls.FlyLookSensitivity : 1f;
            Vector3 euler = camera.transform.rotation.eulerAngles;
            float pitch =
                NormalizeAngle(euler.x) -
                lookDelta.y *
                DeucarianCameraNavigationSpeedResolver.GetFlyRotationSpeed(controls) *
                lookSensitivity;
            float yaw =
                euler.y +
                lookDelta.x *
                DeucarianCameraNavigationSpeedResolver.GetFlyRotationSpeed(controls) *
                lookSensitivity;
            pitch = Mathf.Clamp(pitch, -89f, 89f);
            camera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        }

        private void ApplyZoomInput(
            float zoomDelta,
            float wheelZoomReferenceDistance,
            IDeucarianCameraNavigationControls controls)
        {
            if (!IsFinite(zoomDelta) || Mathf.Abs(zoomDelta) <= 0.0001f)
            {
                return;
            }

            float zoomStep = controls != null
                ? controls.WheelZoomStep
                : DeucarianCameraNavigationControls.DefaultWheelZoomStep;
            float zoomSensitivity =
                controls != null ? controls.FlyZoomSensitivity : 1f;
            float referenceDistance =
                Mathf.Max(MinimumWheelZoomReferenceDistance, wheelZoomReferenceDistance);
            wheelZoom.AddToTarget(
                wheelZoom.Current,
                zoomDelta * referenceDistance * zoomStep * zoomSensitivity);
        }

        private void ApplyZoomSmoothing(
            Camera camera,
            float deltaTime,
            IDeucarianCameraNavigationControls controls)
        {
            float currentZoom = wheelZoom.Current;
            float stopEpsilon = GetZoomStopEpsilon(controls);
            if (!wheelZoom.HasPending(currentZoom, stopEpsilon))
            {
                return;
            }

            float smoothedZoom = wheelZoom.SmoothTowardsTarget(
                currentZoom,
                GetZoomSmoothingTime(controls),
                stopEpsilon,
                deltaTime);
            float zoomDelta = smoothedZoom - currentZoom;
            camera.transform.position += camera.transform.forward * zoomDelta;
        }

        private static float GetSpeedFactor(
            bool boosted,
            bool slowed,
            IDeucarianCameraNavigationControls controls)
        {
            float boostScale = controls != null ? controls.BoostScale : 4f;
            if (boosted)
            {
                return boostScale;
            }

            return slowed ? 1f / boostScale : 1f;
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f)
            {
                angle -= 360f;
            }

            while (angle < -180f)
            {
                angle += 360f;
            }

            return angle;
        }

        private static float GetZoomSmoothingTime(
            IDeucarianCameraNavigationControls controls)
        {
            return controls != null ? controls.WheelZoomSmoothingTime : 0.08f;
        }

        private static float GetZoomStopEpsilon(
            IDeucarianCameraNavigationControls controls)
        {
            return controls != null ? controls.WheelZoomStopEpsilon : 0.001f;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

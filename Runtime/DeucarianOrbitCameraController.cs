using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public sealed class DeucarianOrbitCameraController
    {
        private const float MaxStablePitchDegrees = 75f;
        private const float MaxOrbitDistance = 1000000f;
        private const float DirectionEpsilonSquared = 0.000000000001f;
        private const float MinimumOrthographicSize = 0.01f;
        private const float MaximumOrthographicSize = 100000f;

        private readonly DeucarianWheelZoomSmoother perspectiveZoom =
            new DeucarianWheelZoomSmoother();
        private readonly DeucarianWheelZoomSmoother orthographicZoom =
            new DeucarianWheelZoomSmoother();
        private float referenceScale;

        public Vector3 Pivot { get; private set; }

        public void SetPivot(Vector3 pivot)
        {
            if (IsFinite(pivot))
            {
                Pivot = pivot;
            }
        }

        public void SetReferenceBounds(Bounds bounds)
        {
            SetReferenceScale(bounds.extents.magnitude);
        }

        public void SetReferenceScale(float scale)
        {
            referenceScale = IsFinite(scale) ? Mathf.Max(0f, scale) : 0f;
        }

        public void ClearReferenceBounds()
        {
            referenceScale = 0f;
        }

        public float GetDistanceToPivot(
            Camera camera,
            IDeucarianCameraNavigationControls controls = null)
        {
            return camera == null
                ? GetMinimumDistance(null, controls)
                : GetDistance(camera, controls);
        }

        public float GetMinimumDistance(
            Camera camera,
            IDeucarianCameraNavigationControls controls)
        {
            float absoluteMinimum = controls != null
                ? controls.OrbitMinimumDistance
                : DeucarianCameraNavigationControls.DefaultOrbitMinimumDistance;
            float scaleMultiplier = controls != null
                ? controls.OrbitMinimumDistanceScale
                : DeucarianCameraNavigationControls.DefaultOrbitMinimumDistanceScale;
            float nearClipMultiplier = controls != null
                ? controls.OrbitNearClipDistanceMultiplier
                : DeucarianCameraNavigationControls.DefaultOrbitNearClipDistanceMultiplier;
            float scaleMinimum = referenceScale * scaleMultiplier;
            float nearClipMinimum = camera != null
                ? Mathf.Max(0f, camera.nearClipPlane) * nearClipMultiplier
                : 0f;
            return Mathf.Max(absoluteMinimum, scaleMinimum, nearClipMinimum);
        }

        public void SyncZoomState(
            Camera camera,
            IDeucarianCameraNavigationControls controls = null)
        {
            if (camera == null)
            {
                return;
            }

            if (camera.orthographic)
            {
                orthographicZoom.Reset(camera.orthographicSize);
                return;
            }

            perspectiveZoom.Reset(GetDistance(camera, controls));
        }

        public void Apply(
            Camera camera,
            DeucarianOrbitCameraInput input,
            float deltaTime,
            IDeucarianCameraNavigationControls controls,
            bool allowRotation = true)
        {
            if (camera == null)
            {
                return;
            }

            if (!camera.orthographic && EnsureMinimumDistance(camera, controls))
            {
                perspectiveZoom.Reset(GetDistance(camera, controls));
            }

            if (!input.HasInput && !HasPendingZoom(camera, controls))
            {
                return;
            }

            if (input.HasInput)
            {
                ApplyPan(camera, input, deltaTime, controls);
                ApplyZoomInput(camera, input.Zoom, controls);
                if (allowRotation)
                {
                    ApplyRotation(camera, input.Rotate, controls);
                }
            }

            ApplyZoomSmoothing(camera, deltaTime, controls);
        }

        private void ApplyPan(
            Camera camera,
            DeucarianOrbitCameraInput input,
            float deltaTime,
            IDeucarianCameraNavigationControls controls)
        {
            float distance = GetDistance(camera, controls);
            float speedFactor = GetSpeedFactor(input.Boost, input.Slow, controls);
            float panSensitivity = controls != null ? controls.OrbitPanSensitivity : 1f;

            if (input.Pan.sqrMagnitude > 0.0001f)
            {
                float panScale = camera.orthographic
                    ? Mathf.Max(MinimumOrthographicSize, camera.orthographicSize) *
                      DeucarianCameraNavigationSpeedResolver
                          .GetOrbitOrthographicMousePanSpeed(controls)
                    : distance *
                      DeucarianCameraNavigationSpeedResolver
                          .GetOrbitMousePanSpeed(controls);
                Vector3 pointerOffset =
                    (-camera.transform.right * input.Pan.x -
                     camera.transform.up * input.Pan.y) *
                    panScale *
                    speedFactor *
                    panSensitivity;
                MoveCameraAndPivot(camera, pointerOffset);
            }

            if (input.Move.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            float keyboardScale =
                Mathf.Max(1f, camera.orthographic ? camera.orthographicSize : distance) *
                DeucarianCameraNavigationSpeedResolver
                    .GetOrbitKeyboardPanSpeed(controls) *
                speedFactor *
                panSensitivity *
                Mathf.Max(0f, deltaTime);
            if (camera.orthographic)
            {
                Vector3 orthographicOffset =
                    camera.transform.right * input.Move.x +
                    camera.transform.up * input.Move.z;
                MoveCameraAndPivot(camera, orthographicOffset * keyboardScale);

                if (Mathf.Abs(input.Move.y) > 0.0001f)
                {
                    camera.transform.position +=
                        camera.transform.forward * (input.Move.y * keyboardScale);
                }

                return;
            }

            Vector3 pivotOffset =
                camera.transform.right * input.Move.x +
                camera.transform.up * input.Move.y;
            MoveCameraAndPivot(camera, pivotOffset * keyboardScale);

            if (Mathf.Abs(input.Move.z) > 0.0001f)
            {
                MoveTowardsPivot(camera, input.Move.z * keyboardScale, controls);
                perspectiveZoom.Reset(GetDistance(camera, controls));
            }
        }

        private void ApplyZoomInput(
            Camera camera,
            float zoomDelta,
            IDeucarianCameraNavigationControls controls)
        {
            if (!IsFinite(zoomDelta) || Mathf.Abs(zoomDelta) <= 0.0001f)
            {
                return;
            }

            float zoomStep = controls != null
                ? controls.WheelZoomStep
                : DeucarianCameraNavigationControls.DefaultWheelZoomStep;
            if (camera.orthographic)
            {
                float zoomSensitivity = controls != null
                    ? controls.OrbitZoomSensitivity
                    : 1f;
                orthographicZoom.MultiplyTarget(
                    camera.orthographicSize,
                    zoomDelta,
                    zoomStep,
                    zoomSensitivity,
                    MinimumOrthographicSize,
                    MaximumOrthographicSize);
                return;
            }

            float minimumDistance = GetMinimumDistance(camera, controls);
            float maximumDistance = Mathf.Max(MaxOrbitDistance, minimumDistance);
            perspectiveZoom.MultiplyTarget(
                GetDistance(camera, controls),
                zoomDelta,
                zoomStep,
                controls != null ? controls.OrbitZoomSensitivity : 1f,
                minimumDistance,
                maximumDistance);
        }

        private void ApplyZoomSmoothing(
            Camera camera,
            float deltaTime,
            IDeucarianCameraNavigationControls controls)
        {
            if (camera.orthographic)
            {
                float stopEpsilon = GetZoomStopEpsilon(controls);
                if (!orthographicZoom.HasPending(camera.orthographicSize, stopEpsilon))
                {
                    return;
                }

                camera.orthographicSize = Mathf.Clamp(
                    orthographicZoom.SmoothTowardsTarget(
                        camera.orthographicSize,
                        GetZoomSmoothingTime(controls),
                        stopEpsilon,
                        deltaTime),
                    MinimumOrthographicSize,
                    MaximumOrthographicSize);
                return;
            }

            float minimumDistance = GetMinimumDistance(camera, controls);
            float maximumDistance = Mathf.Max(MaxOrbitDistance, minimumDistance);
            float distance = GetDistance(camera, controls);
            float perspectiveStopEpsilon =
                GetPerspectiveZoomStopEpsilon(controls, minimumDistance);
            perspectiveZoom.ClampTarget(minimumDistance, maximumDistance);
            if (!perspectiveZoom.HasPending(distance, perspectiveStopEpsilon))
            {
                return;
            }

            Vector3 offset = camera.transform.position - Pivot;
            Vector3 direction = ResolveOrbitDirection(camera, offset);
            float smoothedDistance = Mathf.Clamp(
                perspectiveZoom.SmoothTowardsTarget(
                    distance,
                    GetZoomSmoothingTime(controls),
                    perspectiveStopEpsilon,
                    deltaTime),
                minimumDistance,
                maximumDistance);
            Vector3 position = Pivot + direction * smoothedDistance;
            if (IsFinite(position))
            {
                camera.transform.position = position;
            }
            else
            {
                perspectiveZoom.Reset(distance);
            }
        }

        private void ApplyRotation(
            Camera camera,
            Vector2 rotateDelta,
            IDeucarianCameraNavigationControls controls)
        {
            if (!IsFinite(rotateDelta.x) ||
                !IsFinite(rotateDelta.y) ||
                rotateDelta.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Vector3 offset = camera.transform.position - Pivot;
            float distance = GetDistance(camera, controls);
            offset = ResolveOrbitDirection(camera, offset) * distance;

            float rotationSensitivity =
                controls != null ? controls.OrbitRotationSensitivity : 1f;
            if (controls != null && controls.InvertOrbitRotation)
            {
                rotateDelta = -rotateDelta;
            }

            if (controls != null && controls.AllowInfiniteVerticalOrbit)
            {
                ApplyContinuousRotation(
                    camera,
                    offset,
                    distance,
                    rotateDelta,
                    rotationSensitivity,
                    DeucarianCameraNavigationSpeedResolver
                        .GetOrbitRotationSpeed(controls));
                return;
            }

            float yaw =
                Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg +
                rotateDelta.x *
                DeucarianCameraNavigationSpeedResolver
                    .GetOrbitRotationSpeed(controls) *
                rotationSensitivity;
            float pitch =
                Mathf.Asin(Mathf.Clamp(offset.y / distance, -1f, 1f)) * Mathf.Rad2Deg +
                rotateDelta.y *
                DeucarianCameraNavigationSpeedResolver
                    .GetOrbitRotationSpeed(controls) *
                rotationSensitivity;
            pitch = Mathf.Clamp(pitch, -MaxStablePitchDegrees, MaxStablePitchDegrees);

            float yawRadians = yaw * Mathf.Deg2Rad;
            float pitchRadians = pitch * Mathf.Deg2Rad;
            float horizontalDistance = Mathf.Cos(pitchRadians) * distance;
            Vector3 newOffset = new Vector3(
                Mathf.Sin(yawRadians) * horizontalDistance,
                Mathf.Sin(pitchRadians) * distance,
                Mathf.Cos(yawRadians) * horizontalDistance);

            camera.transform.position = Pivot + newOffset;
            LookAtPivot(camera, Vector3.up);
        }

        private void ApplyContinuousRotation(
            Camera camera,
            Vector3 offset,
            float distance,
            Vector2 rotateDelta,
            float rotationSensitivity,
            float rotationSpeed)
        {
            float yawDelta = rotateDelta.x * rotationSpeed * rotationSensitivity;
            float pitchDelta = rotateDelta.y * rotationSpeed * rotationSensitivity;
            Vector3 pitchAxis = camera.transform.right;
            if (pitchAxis.sqrMagnitude <= 0.0001f)
            {
                pitchAxis = Vector3.right;
            }

            Quaternion yawRotation = Quaternion.AngleAxis(yawDelta, Vector3.up);
            Quaternion pitchRotation = Quaternion.AngleAxis(pitchDelta, pitchAxis.normalized);
            Quaternion rotation = yawRotation * pitchRotation;
            Vector3 newOffset = rotation * offset;
            if (!TryNormalize(newOffset, out Vector3 newDirection))
            {
                newDirection = ResolveOrbitDirection(camera, offset);
            }

            camera.transform.position = Pivot + newDirection * distance;
            LookAtPivot(camera, rotation * camera.transform.up);
        }

        private void MoveTowardsPivot(
            Camera camera,
            float distanceDelta,
            IDeucarianCameraNavigationControls controls)
        {
            Vector3 offset = camera.transform.position - Pivot;
            Vector3 direction = ResolveOrbitDirection(camera, offset);
            float distance = GetDistance(camera, controls);
            float minimumDistance = GetMinimumDistance(camera, controls);
            camera.transform.position =
                Pivot + direction * Mathf.Max(minimumDistance, distance - distanceDelta);
        }

        private void MoveCameraAndPivot(Camera camera, Vector3 offset)
        {
            if (!IsFinite(offset))
            {
                return;
            }

            camera.transform.position += offset;
            Pivot += offset;
        }

        private void LookAtPivot(Camera camera, Vector3 desiredUp)
        {
            Vector3 direction = Pivot - camera.transform.position;
            if (!TryNormalize(direction, out Vector3 normalizedDirection))
            {
                return;
            }

            Vector3 up = TryNormalize(desiredUp, out Vector3 normalizedUp)
                ? normalizedUp
                : Vector3.up;
            if (Mathf.Abs(Vector3.Dot(normalizedDirection, up)) > 0.999f)
            {
                up = Vector3.Cross(camera.transform.right, normalizedDirection);
                if (TryNormalize(up, out Vector3 stableUp))
                {
                    up = stableUp;
                }
                else
                {
                    up = Vector3.forward;
                }
            }

            camera.transform.rotation = Quaternion.LookRotation(normalizedDirection, up);
        }

        private float GetDistance(
            Camera camera,
            IDeucarianCameraNavigationControls controls)
        {
            float distance = Vector3.Distance(camera.transform.position, Pivot);
            return IsFinite(distance)
                ? Mathf.Max(distance, GetMinimumDistance(camera, controls))
                : GetMinimumDistance(camera, controls);
        }

        private bool HasPendingZoom(
            Camera camera,
            IDeucarianCameraNavigationControls controls)
        {
            if (camera.orthographic)
            {
                return orthographicZoom.HasPending(
                    camera.orthographicSize,
                    GetZoomStopEpsilon(controls));
            }

            float minimumDistance = GetMinimumDistance(camera, controls);
            float maximumDistance = Mathf.Max(MaxOrbitDistance, minimumDistance);
            perspectiveZoom.ClampTarget(minimumDistance, maximumDistance);
            return perspectiveZoom.HasPending(
                GetDistance(camera, controls),
                GetPerspectiveZoomStopEpsilon(controls, minimumDistance));
        }

        private bool EnsureMinimumDistance(
            Camera camera,
            IDeucarianCameraNavigationControls controls)
        {
            Vector3 offset = camera.transform.position - Pivot;
            float distance = offset.magnitude;
            float minimumDistance = GetMinimumDistance(camera, controls);
            if (IsFinite(distance) && distance >= minimumDistance)
            {
                return false;
            }

            Vector3 direction = ResolveOrbitDirection(camera, offset);
            Vector3 position = Pivot + direction * minimumDistance;
            if (!IsFinite(position))
            {
                return false;
            }

            camera.transform.position = position;
            return true;
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

        private static float GetPerspectiveZoomStopEpsilon(
            IDeucarianCameraNavigationControls controls,
            float minimumDistance)
        {
            return Mathf.Min(
                GetZoomStopEpsilon(controls),
                Mathf.Max(0.000001f, minimumDistance * 0.1f));
        }

        private static Vector3 ResolveOrbitDirection(Camera camera, Vector3 offset)
        {
            if (TryNormalize(offset, out Vector3 direction))
            {
                return direction;
            }

            if (camera != null && TryNormalize(-camera.transform.forward, out direction))
            {
                return direction;
            }

            return Vector3.back;
        }

        private static bool TryNormalize(Vector3 value, out Vector3 normalized)
        {
            float sqrMagnitude = value.sqrMagnitude;
            if (!IsFinite(sqrMagnitude) || sqrMagnitude <= DirectionEpsilonSquared)
            {
                normalized = default;
                return false;
            }

            normalized = value / Mathf.Sqrt(sqrMagnitude);
            if (IsFinite(normalized))
            {
                return true;
            }

            normalized = default;
            return false;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}

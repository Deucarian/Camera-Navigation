using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public static class DeucarianCameraFraming
    {
        private const float DefaultPadding = 1.25f;
        private const float MinimumBoundsRadius = 1f;
        private const float MinimumOrthographicSize = 1f;

        public static bool TryCalculateRendererBounds(GameObject root, out Bounds bounds)
        {
            bounds = default;
            if (root == null)
            {
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
            bool hasBounds = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                }
                else
                {
                    bounds.Encapsulate(renderer.bounds);
                }
            }

            return hasBounds;
        }

        public static DeucarianCameraPose CreatePerspectiveFramePose(
            Bounds bounds,
            Camera camera,
            Vector3 preferredForward,
            float padding = DefaultPadding)
        {
            float fieldOfView = camera != null ? camera.fieldOfView : 60f;
            float aspect = ResolveAspect(camera);
            Vector3 forward = ResolveForward(preferredForward, camera);
            float radius = Mathf.Max(MinimumBoundsRadius, bounds.extents.magnitude) * Mathf.Max(1f, padding);
            float verticalFov = Mathf.Clamp(fieldOfView, 1f, 179f);
            float horizontalFov = Camera.VerticalToHorizontalFieldOfView(verticalFov, aspect);
            float limitingFov = Mathf.Min(verticalFov, horizontalFov) * Mathf.Deg2Rad;
            float distance = radius / Mathf.Sin(Mathf.Clamp(limitingFov * 0.5f, 0.01f, Mathf.PI - 0.01f));
            Vector3 position = bounds.center - forward * distance;

            return new DeucarianCameraPose(
                position,
                CreateLookRotation(forward),
                false,
                camera != null ? camera.orthographicSize : MinimumOrthographicSize,
                fieldOfView);
        }

        public static DeucarianCameraPose CreateTopDownPose(
            Bounds bounds,
            Camera camera,
            Vector3? centerOverride = null,
            float padding = DefaultPadding)
        {
            Vector3 center = centerOverride ?? bounds.center;
            float orthographicSize = CalculateTopDownOrthographicSize(bounds, ResolveAspect(camera), padding);
            float cameraHeight = Mathf.Max(bounds.extents.y + orthographicSize * 2f, MinimumBoundsRadius * 4f);
            Vector3 position = new Vector3(center.x, bounds.max.y + cameraHeight, center.z);
            return new DeucarianCameraPose(
                position,
                Quaternion.LookRotation(Vector3.down, Vector3.forward),
                true,
                orthographicSize,
                camera != null ? camera.fieldOfView : 60f);
        }

        public static DeucarianCameraPose CreateViewDirectionPose(
            Bounds bounds,
            Camera camera,
            Vector3 directionFromTargetToCamera,
            float padding = DefaultPadding)
        {
            Vector3 direction = directionFromTargetToCamera.sqrMagnitude > 0.0001f
                ? directionFromTargetToCamera.normalized
                : new Vector3(1f, 0.65f, -1f).normalized;
            return CreatePerspectiveFramePose(bounds, camera, -direction, padding);
        }

        public static float CalculateTopDownOrthographicSize(Bounds bounds, float aspect, float padding = DefaultPadding)
        {
            float safeAspect = Mathf.Max(0.01f, aspect);
            float halfHeight = Mathf.Max(bounds.extents.z, MinimumOrthographicSize);
            float halfWidth = Mathf.Max(bounds.extents.x, MinimumOrthographicSize);
            return Mathf.Max(halfHeight, halfWidth / safeAspect) * Mathf.Max(1f, padding);
        }

        public static void ConfigureClipPlanes(Camera camera, Bounds bounds)
        {
            if (camera == null)
            {
                return;
            }

            float radius = Mathf.Max(MinimumBoundsRadius, bounds.extents.magnitude);
            float distance = Vector3.Distance(camera.transform.position, bounds.center);
            camera.nearClipPlane = Mathf.Min(camera.nearClipPlane, 0.01f);
            camera.farClipPlane = Mathf.Max(camera.farClipPlane, distance + radius * 4f);
        }

        public static DeucarianCameraPose CreateProjectionMatchedPose(
            DeucarianCameraPose currentProjectionPose,
            DeucarianCameraPose committedTargetPose,
            Vector3 referencePoint,
            float transitionMatchFieldOfView)
        {
            if (currentProjectionPose.Orthographic == committedTargetPose.Orthographic)
            {
                return committedTargetPose;
            }

            return currentProjectionPose.Orthographic
                ? CreateOrthographicMatchPoseForPerspectiveSwitch(
                    currentProjectionPose,
                    referencePoint,
                    transitionMatchFieldOfView)
                : CreatePerspectiveMatchPoseForOrthographicSwitch(
                    committedTargetPose,
                    referencePoint,
                    transitionMatchFieldOfView);
        }

        public static DeucarianCameraPose CreatePerspectiveMatchPoseForOrthographicSwitch(
            DeucarianCameraPose orthographicPose,
            Vector3 referencePoint,
            float transitionMatchFieldOfView)
        {
            float fieldOfView = ClampProjectionMatchFieldOfView(transitionMatchFieldOfView);
            float distance = CalculatePerspectiveDistanceForOrthographicSize(orthographicPose.OrthographicSize, fieldOfView);
            Vector3 position = referencePoint - ResolveSafeForward(orthographicPose.Rotation * Vector3.forward) * distance;
            return new DeucarianCameraPose(position, orthographicPose.Rotation, false, orthographicPose.OrthographicSize, fieldOfView);
        }

        public static DeucarianCameraPose CreateVisibleTopDownTransitionPose(
            DeucarianCameraPose topDownPose,
            Vector3 referencePoint,
            float preferredFieldOfView)
        {
            float fieldOfView = Mathf.Clamp(preferredFieldOfView, 15f, 30f);
            float distance = CalculatePerspectiveDistanceForOrthographicSize(
                topDownPose.OrthographicSize,
                fieldOfView);
            Vector3 position = referencePoint - ResolveSafeForward(topDownPose.Rotation * Vector3.forward) * distance;
            return new DeucarianCameraPose(
                position,
                topDownPose.Rotation,
                false,
                topDownPose.OrthographicSize,
                fieldOfView);
        }

        public static DeucarianCameraPose CreateOrthographicMatchPoseForPerspectiveSwitch(
            DeucarianCameraPose orthographicPose,
            Vector3 referencePoint,
            float transitionMatchFieldOfView)
        {
            DeucarianCameraPose perspectiveMatch = CreatePerspectiveMatchPoseForOrthographicSwitch(
                orthographicPose,
                referencePoint,
                transitionMatchFieldOfView);
            return new DeucarianCameraPose(
                perspectiveMatch.Position,
                perspectiveMatch.Rotation,
                true,
                orthographicPose.OrthographicSize,
                orthographicPose.FieldOfView);
        }

        public static DeucarianCameraPose CreateOrthographicCommitPose(
            DeucarianCameraPose perspectiveMatchPose,
            DeucarianCameraPose orthographicTargetPose)
        {
            return new DeucarianCameraPose(
                perspectiveMatchPose.Position,
                perspectiveMatchPose.Rotation,
                true,
                orthographicTargetPose.OrthographicSize,
                orthographicTargetPose.FieldOfView);
        }

        public static DeucarianCameraPose CreatePerspectiveSwitchPose(
            DeucarianCameraPose orthographicMatchPose,
            float transitionMatchFieldOfView)
        {
            return new DeucarianCameraPose(
                orthographicMatchPose.Position,
                orthographicMatchPose.Rotation,
                false,
                orthographicMatchPose.OrthographicSize,
                ClampProjectionMatchFieldOfView(transitionMatchFieldOfView));
        }

        public static Quaternion CreateLookAtRotation(
            Vector3 cameraPosition,
            Vector3 target,
            Quaternion preferredUpRotation)
        {
            Vector3 forward = target - cameraPosition;
            Vector3 preferredUp = preferredUpRotation == default
                ? Vector3.up
                : preferredUpRotation * Vector3.up;
            return CreateLookRotation(forward, preferredUp);
        }

        public static float CalculatePerspectiveDistanceForOrthographicSize(float orthographicSize, float fieldOfView)
        {
            return Mathf.Max(0.01f, orthographicSize) /
                   Mathf.Tan(ClampProjectionMatchFieldOfView(fieldOfView) * Mathf.Deg2Rad * 0.5f);
        }

        public static float CalculatePerspectiveVisibleHalfHeight(float distance, float fieldOfView)
        {
            return Mathf.Tan(ClampProjectionMatchFieldOfView(fieldOfView) * Mathf.Deg2Rad * 0.5f) *
                   Mathf.Max(0.01f, distance);
        }

        public static float CalculatePerspectiveFieldOfViewForOrthographicMatch(
            DeucarianCameraPose orthographicPose,
            Vector3 referencePoint)
        {
            float distance = CalculateViewDepth(
                orthographicPose.Position,
                orthographicPose.Rotation * Vector3.forward,
                referencePoint);
            if (distance <= 0.0001f)
            {
                return orthographicPose.FieldOfView;
            }

            float fieldOfView = Mathf.Atan(orthographicPose.OrthographicSize / distance) * Mathf.Rad2Deg * 2f;
            return Mathf.Clamp(fieldOfView, 0.01f, 179f);
        }

        public static float CalculateOrthographicSizeForPerspectiveMatch(
            DeucarianCameraPose perspectivePose,
            Vector3 referencePoint)
        {
            float distance = CalculateViewDepth(
                perspectivePose.Position,
                perspectivePose.Rotation * Vector3.forward,
                referencePoint);
            if (distance <= 0.0001f)
            {
                return perspectivePose.OrthographicSize;
            }

            float orthographicSize =
                Mathf.Tan(perspectivePose.FieldOfView * Mathf.Deg2Rad * 0.5f) * distance;
            return Mathf.Max(0.01f, orthographicSize);
        }

        private static float ResolveAspect(Camera camera)
        {
            if (camera != null && camera.aspect > 0.01f)
            {
                return camera.aspect;
            }

            return Screen.height > 0 ? Mathf.Max(0.01f, Screen.width / (float)Screen.height) : 1.777778f;
        }

        private static Vector3 ResolveForward(Vector3 preferredForward, Camera camera)
        {
            Vector3 forward = preferredForward;
            if (forward.sqrMagnitude <= 0.0001f && camera != null)
            {
                forward = camera.transform.forward;
            }

            return forward.sqrMagnitude > 0.0001f
                ? forward.normalized
                : new Vector3(-1f, -0.6f, 1f).normalized;
        }

        private static Quaternion CreateLookRotation(Vector3 forward)
        {
            return CreateLookRotation(forward, Vector3.up);
        }

        private static Quaternion CreateLookRotation(Vector3 forward, Vector3 preferredUp)
        {
            Vector3 normalizedForward = ResolveSafeForward(forward);
            Vector3 up = preferredUp.sqrMagnitude > 0.0001f
                ? preferredUp.normalized
                : Vector3.up;
            if (Mathf.Abs(Vector3.Dot(normalizedForward, up)) > 0.98f)
            {
                up = Mathf.Abs(Vector3.Dot(normalizedForward, Vector3.forward)) > 0.98f
                    ? Vector3.up
                    : Vector3.forward;
            }

            return Quaternion.LookRotation(normalizedForward, up);
        }

        private static float CalculateViewDepth(Vector3 position, Vector3 forward, Vector3 referencePoint)
        {
            Vector3 normalizedForward = ResolveSafeForward(forward);
            float distance = Mathf.Abs(Vector3.Dot(referencePoint - position, normalizedForward));
            if (distance <= 0.0001f)
            {
                distance = Vector3.Distance(referencePoint, position);
            }

            return distance;
        }

        private static Vector3 ResolveSafeForward(Vector3 forward)
        {
            return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        }

        private static float ClampProjectionMatchFieldOfView(float fieldOfView)
        {
            return Mathf.Clamp(fieldOfView, 0.1f, 30f);
        }
    }
}

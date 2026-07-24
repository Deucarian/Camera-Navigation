using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public static partial class DeucarianCameraFraming
    {
        private const float MinimumFrameExtent = 0.0001f;

        public static bool TryCreateCurrentProjectionFramePose(
            DeucarianCameraFramingTarget target,
            Camera camera,
            out DeucarianCameraPose pose)
        {
            pose = default;
            if (camera == null ||
                !IsUsableFrameBounds(target.Bounds) ||
                !IsFiniteFrameValue(target.FocusPoint) ||
                !IsFiniteFrameValue(target.Padding) ||
                !TryResolveFrameRotation(
                    target,
                    camera.transform.rotation,
                    out Quaternion rotation))
            {
                return false;
            }

            float padding = Mathf.Max(1f, target.Padding);
            float aspect = ResolveAspect(camera);
            float nearClearance =
                Mathf.Max(MinimumFrameExtent, camera.nearClipPlane) +
                Mathf.Max(
                    MinimumFrameExtent,
                    camera.nearClipPlane * 0.05f);
            Vector3 right = rotation * Vector3.right;
            Vector3 up = rotation * Vector3.up;
            Vector3 forward = rotation * Vector3.forward;

            if (camera.orthographic)
            {
                CalculateOrthographicFit(
                    target,
                    right,
                    up,
                    forward,
                    padding,
                    aspect,
                    nearClearance,
                    out float orthographicSize,
                    out float distance);
                Vector3 position =
                    target.FocusPoint - forward * distance;
                pose = new DeucarianCameraPose(
                    position,
                    rotation,
                    true,
                    orthographicSize,
                    camera.fieldOfView);
                return IsFiniteFrameValue(position);
            }

            float perspectiveDistance = CalculatePerspectiveFitDistance(
                target,
                right,
                up,
                forward,
                padding,
                aspect,
                camera.fieldOfView,
                nearClearance);
            Vector3 perspectivePosition =
                target.FocusPoint - forward * perspectiveDistance;
            pose = new DeucarianCameraPose(
                perspectivePosition,
                rotation,
                false,
                camera.orthographicSize,
                camera.fieldOfView);
            return IsFiniteFrameValue(perspectivePosition);
        }

        private static float CalculatePerspectiveFitDistance(
            DeucarianCameraFramingTarget target,
            Vector3 right,
            Vector3 up,
            Vector3 forward,
            float padding,
            float aspect,
            float fieldOfView,
            float nearClearance)
        {
            float verticalHalfAngle =
                Mathf.Clamp(fieldOfView, 1f, 179f) *
                Mathf.Deg2Rad *
                0.5f;
            float verticalTangent =
                Mathf.Max(0.0001f, Mathf.Tan(verticalHalfAngle));
            float horizontalTangent =
                Mathf.Max(0.0001f, verticalTangent * aspect);
            float requiredDistance = MinimumFrameExtent;

            VisitFrameCorners(
                target,
                right,
                up,
                forward,
                (horizontal, vertical, depth) =>
                {
                    requiredDistance = Mathf.Max(
                        requiredDistance,
                        Mathf.Abs(horizontal) * padding /
                        horizontalTangent -
                        depth,
                        Mathf.Abs(vertical) * padding /
                        verticalTangent -
                        depth,
                        nearClearance - depth);
                });
            return Mathf.Max(MinimumFrameExtent, requiredDistance);
        }

        private static void CalculateOrthographicFit(
            DeucarianCameraFramingTarget target,
            Vector3 right,
            Vector3 up,
            Vector3 forward,
            float padding,
            float aspect,
            float nearClearance,
            out float orthographicSize,
            out float distance)
        {
            float requiredHalfWidth = MinimumFrameExtent;
            float requiredHalfHeight = MinimumFrameExtent;
            float requiredDistance = MinimumFrameExtent;
            VisitFrameCorners(
                target,
                right,
                up,
                forward,
                (horizontal, vertical, depth) =>
                {
                    requiredHalfWidth = Mathf.Max(
                        requiredHalfWidth,
                        Mathf.Abs(horizontal));
                    requiredHalfHeight = Mathf.Max(
                        requiredHalfHeight,
                        Mathf.Abs(vertical));
                    requiredDistance = Mathf.Max(
                        requiredDistance,
                        nearClearance - depth);
                });

            orthographicSize = Mathf.Max(
                MinimumFrameExtent,
                Mathf.Max(
                    requiredHalfHeight,
                    requiredHalfWidth / aspect) *
                padding);
            distance = Mathf.Max(MinimumFrameExtent, requiredDistance);
        }

        private static void VisitFrameCorners(
            DeucarianCameraFramingTarget target,
            Vector3 right,
            Vector3 up,
            Vector3 forward,
            System.Action<float, float, float> visitor)
        {
            Vector3 min = target.Bounds.min;
            Vector3 max = target.Bounds.max;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 corner = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        Vector3 offset =
                            corner - target.FocusPoint;
                        visitor(
                            Vector3.Dot(offset, right),
                            Vector3.Dot(offset, up),
                            Vector3.Dot(offset, forward));
                    }
                }
            }
        }

        private static bool TryResolveFrameRotation(
            DeucarianCameraFramingTarget target,
            Quaternion currentRotation,
            out Quaternion rotation)
        {
            rotation = target.HasPreferredCameraRotation
                ? target.PreferredCameraRotation
                : currentRotation;
            float magnitude = Mathf.Sqrt(
                rotation.x * rotation.x +
                rotation.y * rotation.y +
                rotation.z * rotation.z +
                rotation.w * rotation.w);
            if (!IsFiniteFrameValue(magnitude) ||
                magnitude <= MinimumFrameExtent)
            {
                rotation = Quaternion.identity;
                return false;
            }

            float inverseMagnitude = 1f / magnitude;
            rotation = new Quaternion(
                rotation.x * inverseMagnitude,
                rotation.y * inverseMagnitude,
                rotation.z * inverseMagnitude,
                rotation.w * inverseMagnitude);
            return true;
        }

        private static bool IsUsableFrameBounds(Bounds bounds)
        {
            return IsFiniteFrameValue(bounds.center) &&
                   IsFiniteFrameValue(bounds.size) &&
                   bounds.size.x >= 0f &&
                   bounds.size.y >= 0f &&
                   bounds.size.z >= 0f &&
                   bounds.size.sqrMagnitude >
                   MinimumFrameExtent * MinimumFrameExtent;
        }

        private static bool IsFiniteFrameValue(Vector3 value)
        {
            return IsFiniteFrameValue(value.x) &&
                   IsFiniteFrameValue(value.y) &&
                   IsFiniteFrameValue(value.z);
        }

        private static bool IsFiniteFrameValue(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}

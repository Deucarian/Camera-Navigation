using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public static partial class DeucarianCameraFraming
    {
        private const float MinimumPosedBoundsSize = 0.000001f;

        public static bool TryCreateWorldBounds(
            DeucarianPosedBounds posedBounds,
            out Bounds worldBounds)
        {
            worldBounds = default;
            Bounds localBounds = posedBounds.LocalBounds;
            if (!IsFinitePosedBoundsValue(posedBounds.Position) ||
                !IsUsablePosedBoundsRotation(posedBounds.Rotation) ||
                !IsFinitePosedBoundsValue(localBounds.center) ||
                !IsFinitePosedBoundsValue(localBounds.size) ||
                localBounds.size.x < 0f ||
                localBounds.size.y < 0f ||
                localBounds.size.z < 0f ||
                localBounds.size.sqrMagnitude <=
                MinimumPosedBoundsSize * MinimumPosedBoundsSize)
            {
                return false;
            }

            Quaternion rotation = Normalize(posedBounds.Rotation);
            Vector3 center =
                posedBounds.Position + rotation * localBounds.center;
            Vector3 extents = localBounds.extents;
            bool hasCorner = false;
            for (int x = -1; x <= 1; x += 2)
            {
                for (int y = -1; y <= 1; y += 2)
                {
                    for (int z = -1; z <= 1; z += 2)
                    {
                        Vector3 localOffset = new Vector3(
                            extents.x * x,
                            extents.y * y,
                            extents.z * z);
                        Vector3 corner =
                            center + rotation * localOffset;
                        if (!IsFinitePosedBoundsValue(corner))
                        {
                            worldBounds = default;
                            return false;
                        }

                        if (!hasCorner)
                        {
                            worldBounds =
                                new Bounds(corner, Vector3.zero);
                            hasCorner = true;
                        }
                        else
                        {
                            worldBounds.Encapsulate(corner);
                        }
                    }
                }
            }

            return hasCorner &&
                   IsFinitePosedBoundsValue(worldBounds.center) &&
                   IsFinitePosedBoundsValue(worldBounds.size);
        }

        private static Quaternion Normalize(Quaternion rotation)
        {
            float magnitude = Mathf.Sqrt(
                rotation.x * rotation.x +
                rotation.y * rotation.y +
                rotation.z * rotation.z +
                rotation.w * rotation.w);
            return new Quaternion(
                rotation.x / magnitude,
                rotation.y / magnitude,
                rotation.z / magnitude,
                rotation.w / magnitude);
        }

        private static bool IsUsablePosedBoundsRotation(
            Quaternion rotation)
        {
            if (!IsFinitePosedBoundsValue(rotation.x) ||
                !IsFinitePosedBoundsValue(rotation.y) ||
                !IsFinitePosedBoundsValue(rotation.z) ||
                !IsFinitePosedBoundsValue(rotation.w))
            {
                return false;
            }

            float magnitudeSquared =
                rotation.x * rotation.x +
                rotation.y * rotation.y +
                rotation.z * rotation.z +
                rotation.w * rotation.w;
            return magnitudeSquared >
                   MinimumPosedBoundsSize *
                   MinimumPosedBoundsSize;
        }

        private static bool IsFinitePosedBoundsValue(
            Vector3 value)
        {
            return IsFinitePosedBoundsValue(value.x) &&
                   IsFinitePosedBoundsValue(value.y) &&
                   IsFinitePosedBoundsValue(value.z);
        }

        private static bool IsFinitePosedBoundsValue(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}

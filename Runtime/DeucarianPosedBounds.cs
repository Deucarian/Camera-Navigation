using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public readonly struct DeucarianPosedBounds
    {
        public DeucarianPosedBounds(
            Vector3 position,
            Quaternion rotation,
            Bounds localBounds)
        {
            Position = position;
            Rotation = rotation;
            LocalBounds = localBounds;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public Bounds LocalBounds { get; }
    }
}

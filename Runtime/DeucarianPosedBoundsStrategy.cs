using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public sealed class DeucarianPosedBoundsStrategy :
        IDeucarianFramingBoundsStrategy<DeucarianPosedBounds>
    {
        private DeucarianPosedBoundsStrategy()
        {
        }

        public static DeucarianPosedBoundsStrategy Instance { get; } =
            new DeucarianPosedBoundsStrategy();

        public bool TryGetBounds(
            DeucarianPosedBounds source,
            out Bounds bounds)
        {
            return DeucarianCameraFraming.TryCreateWorldBounds(
                source,
                out bounds);
        }
    }
}

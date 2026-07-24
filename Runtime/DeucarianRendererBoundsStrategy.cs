using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public sealed class DeucarianRendererBoundsStrategy :
        IDeucarianFramingBoundsStrategy<GameObject>
    {
        private DeucarianRendererBoundsStrategy()
        {
        }

        public static DeucarianRendererBoundsStrategy Instance { get; } =
            new DeucarianRendererBoundsStrategy();

        public bool TryGetBounds(
            GameObject source,
            out Bounds bounds)
        {
            return DeucarianCameraFraming.TryCalculateRendererBounds(
                source,
                out bounds);
        }
    }
}

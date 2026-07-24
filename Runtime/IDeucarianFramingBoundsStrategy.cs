using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public interface IDeucarianFramingBoundsStrategy<in TSource>
    {
        bool TryGetBounds(TSource source, out Bounds bounds);
    }
}

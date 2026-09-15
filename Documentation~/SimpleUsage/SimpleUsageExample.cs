using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
namespace Deucarian.CameraNavigation.Samples.SimpleUsage
{
    public sealed class SimpleUsageExample : MonoBehaviour
    {
        [SerializeField] private CameraNavigationHost navigation;
        [SerializeField] private CameraPresetKey preset = CameraPresets.Overview;
        public Task<CameraMoveResult> FrameAsync(Bounds bounds, CancellationToken cancellationToken = default) =>
            navigation.FrameAsync(bounds, preset, cancellationToken);
    }
}

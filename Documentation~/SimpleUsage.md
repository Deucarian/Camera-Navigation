# Simple usage

Copy the reference example into your project, add SimpleUsageExample and assign its scoped host references. Its serialized definition fields use the same typed keys as code.

Assign the navigator's TargetCamera and configure CameraNavigationHost once with that navigator and `new[] { new CameraPresetDefinition(CameraPresets.Overview) }`. The existing navigator owns movement. Await FrameAsync on the Unity main thread; it completes on completion, cancellation, replacement, disable or target loss. The caller does not poll a coroutine.

Definitions are authored once in SampleDefinitions.cs where applicable; the caller never invents an ID. Replace the sample set with your project's central definitions. A selected key proves its identity and payload type; startup still needs to bind that definition in the correct scope. Missing configuration reports how to fix it. Dynamic targets and choices are issued by their owner instead of selected from a definition dropdown.
```csharp
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
```

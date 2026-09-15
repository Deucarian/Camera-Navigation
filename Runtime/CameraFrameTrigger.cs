using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Deucarian.CameraNavigation
{
    [AddComponentMenu("Deucarian/Camera Navigation/Camera Frame Trigger")]
    public sealed class CameraFrameTrigger : MonoBehaviour
    {
        [SerializeField] private CameraNavigationHost host;
        [SerializeField] private CameraPresetKey preset;
        [SerializeField] private Renderer target;
        [SerializeField] private UnityEvent completed = new UnityEvent();
        [SerializeField] private UnityEvent<string> failed = new UnityEvent<string>();
        public string LastError { get; private set; }
        public Task<CameraMoveResult> FrameAsync(CancellationToken cancellationToken = default)
        {
            if (host == null || target == null) throw new InvalidOperationException("Assign a CameraNavigationHost and target Renderer to CameraFrameTrigger '" + name + "'.");
            return host.FrameAsync(target.bounds, preset, cancellationToken);
        }
        public async void Frame()
        {
            try
            {
                var result = await FrameAsync();
                if (this == null) return;
                if (result == CameraMoveResult.Completed) { LastError = null; completed.Invoke(); }
                else if (result == CameraMoveResult.InvalidTarget) { LastError = "The target bounds cannot be framed. Check the target Renderer and camera."; failed.Invoke(LastError); }
            }
            catch (OperationCanceledException) { }
            catch (Exception error) { if (this != null) { LastError = error.Message; failed.Invoke(LastError); } throw; }
        }
    }
}

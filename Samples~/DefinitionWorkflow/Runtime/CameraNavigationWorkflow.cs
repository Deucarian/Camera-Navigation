using System;
using UnityEngine;

namespace Deucarian.CameraNavigation.Samples.DefinitionWorkflow
{
    /// <summary>Small caller example. The configured scene hosts own services and resource lifetimes.</summary>
    public sealed class CameraNavigationWorkflow : MonoBehaviour
    {
        [SerializeField] private CameraNavigationHost host;
        [SerializeField] private CameraPresetKey preset;
        [SerializeField] private Renderer target;
        [SerializeField] private CameraFrameTrigger trigger;
        private string status = "Ready. Choose an action below.";
        public string Status => status;
        public async void Frame() { status = "Frame: " + await host.FrameAsync(target.bounds, preset); }
        public void FrameComponent() { trigger.Frame(); status = "Framing through CameraFrameTrigger."; }
        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, Math.Min(540, Screen.width - 48), Screen.height - 48), GUI.skin.box);
            GUILayout.Label("Camera-Navigation — definition workflow");
            GUILayout.Label("The typed preset holds framing defaults; the scene supplies the camera and target bounds. Both entry points use the same navigator.");
            GUILayout.Space(12);
            if (GUILayout.Button("Frame with C#", GUILayout.Height(32))) { try { Frame(); } catch (Exception error) { status = error.Message; } }
            if (GUILayout.Button("Frame with component", GUILayout.Height(32))) { try { FrameComponent(); } catch (Exception error) { status = error.Message; } }
            GUILayout.Space(12);
            GUILayout.Label(status);
            GUILayout.EndArea();
        }
    }
}

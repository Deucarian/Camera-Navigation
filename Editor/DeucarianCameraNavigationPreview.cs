using System;
using Deucarian.Common;
using Deucarian.Editor;
using UnityEngine;

namespace Deucarian.CameraNavigation.Editor
{
    /// <summary>An isolated editor camera using the runtime controllers and transition clock.</summary>
    public sealed class DeucarianCameraNavigationPreview : IDisposable
    {
        private readonly CameraNavigationPreviewScene scene;
        private readonly Func<IDeucarianCameraNavigationControls> readControls;
        private readonly DeucarianOrbitCameraController orbit = new DeucarianOrbitCameraController();
        private readonly DeucarianFlyCameraController fly = new DeucarianFlyCameraController();
        private readonly DeucarianCameraNavigator navigator;
        private readonly DeucarianCameraMotionSettings motion;
        private readonly DeucarianCameraNavigationControls fallbackControls;
        private readonly CameraNavigationPreviewInput input;
        private bool disposed;
        private bool flyMode;
        private Matrix4x4 lastViewMatrix, lastProjectionMatrix;

        public DeucarianEditorSpatialPreview View { get; }
        public Camera Camera { get; }
        public Bounds Bounds => scene.Bounds;
        public GameObject PreviewCube => scene.Cube;
        public Vector3 Pivot => orbit.Pivot;
        public bool IsMoving => !disposed && navigator.IsMoving;
        public event Action InputStarted;

        public bool FlyMode
        {
            get => flyMode;
            set { if (flyMode == value) return; flyMode = value; ClearInput(); SyncNavigationState(Pivot); }
        }

        public DeucarianCameraNavigationPreview(Func<IDeucarianCameraNavigationControls> controls,
            Func<float> pointerDeltaScale = null, Func<float> scrollNormalization = null)
        {
            readControls = controls ?? throw new ArgumentNullException(nameof(controls));
            scene = new CameraNavigationPreviewScene();
            Camera = scene.Camera;
            var owner = Camera.gameObject;
            Camera.enabled = false;
            Camera.nearClipPlane = 0.01f;
            Camera.farClipPlane = 1000;
            Camera.aspect = 1.5f;
            Camera.transform.SetPositionAndRotation(new Vector3(5, 3, -6), Quaternion.LookRotation(new Vector3(-5, -3, 6)));
            fallbackControls = ScriptableObject.CreateInstance<DeucarianCameraNavigationControls>();
            fallbackControls.hideFlags = HideFlags.HideAndDontSave;
            motion = DeucarianCameraMotionSettings.CreateRuntimeDefault();
            motion.hideFlags = HideFlags.HideAndDontSave;
            navigator = owner.AddComponent<DeucarianCameraNavigator>();
            navigator.TargetCamera = Camera;
            navigator.MotionSettings = motion;
            navigator.SetManualUpdates(true);
            navigator.CaptureOrigin();
            orbit.SetReferenceBounds(Bounds);
            SyncNavigationState(Bounds.center);
            View = new DeucarianEditorSpatialPreview();
            View.SetRenderedTexture(null);
            View.SetCamera(Camera);
            View.tooltip = "Drag to look or orbit. Middle-drag or Shift-drag to pan. Scroll to zoom. Click, then use WASD and Q/E to move; Shift boosts, Ctrl slows.";
            input = new CameraNavigationPreviewInput(View, () => FlyMode, pointerDeltaScale, scrollNormalization);
            View.RegisterCallback<UnityEngine.UIElements.GeometryChangedEvent>(_ => RefreshView(true));
        }

        public void Update(float deltaTime, bool suppressIdleMotion = false)
        {
            if (disposed || deltaTime <= 0 || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
            deltaTime = Mathf.Min(deltaTime, 0.05f);
            var controls = readControls() ?? fallbackControls;
            input.Read(out var orbitInput, out var flyInput);
            ApplyInput(orbitInput, flyInput, deltaTime, controls, suppressIdleMotion);
            bool wasMoving = navigator.IsMoving;
            navigator.Tick(deltaTime);
            if (wasMoving && !navigator.IsMoving) SyncNavigationState(Bounds.center);
            RefreshView();
        }

        /// <summary>Feeds the same runtime input values as the preview gestures; useful for deterministic tests.</summary>
        public void ApplyInput(DeucarianOrbitCameraInput orbitInput, DeucarianFlyCameraInput flyInput, float deltaTime)
            => ApplyInput(orbitInput, flyInput, deltaTime, readControls() ?? fallbackControls);

        private void ApplyInput(DeucarianOrbitCameraInput orbitInput, DeucarianFlyCameraInput flyInput,
            float deltaTime, IDeucarianCameraNavigationControls controls, bool suppressIdleMotion = false)
        {
            if (disposed) return;
            bool hasInput = FlyMode ? flyInput.HasInput : orbitInput.HasInput;
            if (hasInput)
            {
                if (navigator.IsMoving)
                {
                    navigator.CancelMove();
                    SyncNavigationState(Pivot);
                }
                InputStarted?.Invoke();
            }
            if (suppressIdleMotion && !hasInput) return;
            if (navigator.IsMoving) return;
            if (FlyMode) fly.Apply(Camera, flyInput, deltaTime, controls, Vector3.Distance(Camera.transform.position, Pivot));
            else orbit.Apply(Camera, orbitInput, deltaTime, controls, !Camera.orthographic);
        }

        public void Frame(IDeucarianCameraFramingSettings framing = null)
        {
            if (disposed) return;
            StopMotion();
            if (DeucarianCameraFraming.TryCreateCurrentProjectionFramePose(
                new DeucarianCameraFramingTarget(Bounds, Bounds.center), Camera, framing, out var pose))
                navigator.MoveToPose(pose, Bounds, Bounds.center);
        }

        public void Reset()
        {
            if (disposed) return;
            StopMotion();
            navigator.MoveToOrigin(Bounds);
        }

        public void TopDown()
        {
            if (disposed) return;
            StopMotion();
            navigator.MoveToTopDown(Bounds);
        }

        public void StopMotion()
        {
            if (disposed) return;
            navigator.CancelMove();
            ClearInput();
            SyncNavigationState(Pivot);
        }

        public void ClearInput() => input?.Clear();

        public void SyncNavigationState(Vector3 pivot)
        {
            if (disposed) return;
            orbit.SetPivot(pivot);
            orbit.SyncZoomState(Camera, readControls() ?? fallbackControls);
            fly.SyncZoomState();
        }

        public void RefreshView(bool force = false)
        {
            if (disposed) return;
            if (View.contentRect.width > 0 && View.contentRect.height > 0)
                Camera.aspect = View.contentRect.width / View.contentRect.height;
            var viewMatrix = Camera.worldToCameraMatrix;
            var projectionMatrix = Camera.projectionMatrix;
            if (!force && lastViewMatrix == viewMatrix && lastProjectionMatrix == projectionMatrix) return;
            lastViewMatrix = viewMatrix;
            lastProjectionMatrix = projectionMatrix;
            View.SetCamera(Camera);
            if (View.panel != null) View.SetRenderedTexture(scene.Render(View.contentRect));
        }

        public void Dispose()
        {
            if (disposed) return;
            StopMotion();
            disposed = true;
            input.Dispose();
            View.SetCamera(null);
            View.SetRenderedTexture(null);
            UnityObjectUtility.DestroySafely(motion);
            UnityObjectUtility.DestroySafely(fallbackControls);
            scene.Dispose();
            InputStarted = null;
        }
    }
}

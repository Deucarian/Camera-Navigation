using System;
using System.Collections;
using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public sealed class DeucarianCameraNavigator : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private DeucarianCameraMotionSettings motionSettings;

        private Coroutine activeMove;
        private IEnumerator manualMove;
        private float manualDeltaTime;
        private DeucarianCameraPose originPose;
        private bool hasOriginPose;

        public event Action<DeucarianCameraPose> MoveCompleted;
        public event Action MoveCanceled;

        public Camera TargetCamera
        {
            get => targetCamera;
            set => targetCamera = value;
        }

        public DeucarianCameraMotionSettings MotionSettings
        {
            get => ResolveMotionSettings();
            set => motionSettings = value;
        }

        public bool IsMoving => activeMove != null || manualMove != null;
        public bool UsesManualUpdates { get; private set; }

        /// <summary>Opt in to one explicit clock (for isolated previews or deterministic hosts).</summary>
        public void SetManualUpdates(bool enabled)
        {
            if (UsesManualUpdates == enabled) return;
            StopActiveMove(true);
            UsesManualUpdates = enabled;
        }

        public void Tick(float deltaTime)
        {
            if (!UsesManualUpdates || manualMove == null || deltaTime <= 0 ||
                float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
            manualDeltaTime = deltaTime;
            var move = manualMove;
            if (!move.MoveNext() && ReferenceEquals(move, manualMove)) manualMove = null;
        }

        public void CaptureOrigin()
        {
            originPose = DeucarianCameraPose.Capture(ResolveCamera());
            hasOriginPose = true;
        }

        public bool MoveToPose(DeucarianCameraPose targetPose, Bounds bounds, Vector3 pivot, bool animate = true)
        {
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return false;
            }

            StopActiveMove(false);
            DeucarianCameraMotionSettings settings = ResolveMotionSettings();
            DeucarianCameraPose start = DeucarianCameraPose.Capture(camera);
            float duration = animate
                ? settings.CalculateTransitionDuration(Vector3.Distance(start.Position, targetPose.Position))
                : 0f;
            if ((!Application.isPlaying && !UsesManualUpdates) || duration <= 0f)
            {
                targetPose.ApplyTo(camera);
                DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
                MoveCompleted?.Invoke(targetPose);
                return true;
            }

            StartMove(AnimatePose(start, targetPose, bounds, pivot, duration));
            return true;
        }

        public bool MoveToOrigin(Bounds bounds, bool animate = true)
        {
            if (!hasOriginPose)
            {
                CaptureOrigin();
            }

            return MoveToPose(originPose, bounds, bounds.center, animate);
        }

        public bool MoveThroughWaypoints(DeucarianCameraPose[] waypoints, Bounds bounds, bool animate = true)
        {
            if (waypoints == null || waypoints.Length == 0)
            {
                return false;
            }

            StopActiveMove(false);
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return false;
            }

            StartMove(AnimateWaypoints(waypoints, bounds, animate));
            return true;
        }

        public bool MoveToTopDown(Bounds bounds, Vector3? center = null, bool animate = true)
        {
            DeucarianCameraPose pose = DeucarianCameraFraming.CreateTopDownPose(bounds, ResolveCamera(), center);
            return MoveToPose(pose, bounds, center ?? bounds.center, animate);
        }

        public void CancelMove()
        {
            StopActiveMove(true);
        }

        private IEnumerator AnimatePose(
            DeucarianCameraPose start,
            DeucarianCameraPose target,
            Bounds bounds,
            Vector3 pivot,
            float duration,
            bool completeAtEnd = true)
        {
            Camera camera = ResolveCamera();
            bool enteringOrthographic = !start.Orthographic && target.Orthographic;
            bool exitingOrthographic = start.Orthographic && !target.Orthographic;
            var animationStart = exitingOrthographic
                ? DeucarianCameraFraming.CreateVisibleTopDownTransitionPose(start, pivot, target.FieldOfView) : start;
            var animationTarget = enteringOrthographic
                ? DeucarianCameraFraming.CreateVisibleTopDownTransitionPose(target, pivot, start.FieldOfView) : target;
            if (exitingOrthographic) animationStart.ApplyTo(camera);
            float elapsed = 0f;
            while (camera != null && elapsed < duration)
            {
                float movement = ResolveMotionSettings().EvaluateMovement(elapsed / duration);
                float rotation = ResolveMotionSettings().EvaluateRotation(elapsed / duration);
                DeucarianCameraPose current = new DeucarianCameraPose(
                    Vector3.LerpUnclamped(animationStart.Position, animationTarget.Position, movement),
                    Quaternion.Slerp(animationStart.Rotation, animationTarget.Rotation, rotation),
                    animationStart.Orthographic,
                    Mathf.Lerp(animationStart.OrthographicSize, animationTarget.OrthographicSize, movement),
                    Mathf.Lerp(animationStart.FieldOfView, animationTarget.FieldOfView, movement));
                current.ApplyTo(camera);
                DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
                elapsed += UsesManualUpdates ? manualDeltaTime : Time.deltaTime;
                yield return null;
            }

            if (camera != null)
            {
                target.ApplyTo(camera);
                DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
                if (completeAtEnd)
                {
                    MoveCompleted?.Invoke(target);
                }
            }

            if (completeAtEnd)
            {
                activeMove = null;
            }
        }

        private IEnumerator AnimateWaypoints(DeucarianCameraPose[] waypoints, Bounds bounds, bool animate)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                Camera camera = ResolveCamera();
                if (camera == null)
                {
                    yield break;
                }

                DeucarianCameraPose start = DeucarianCameraPose.Capture(camera);
                DeucarianCameraPose target = waypoints[i];
                float duration = animate
                    ? ResolveMotionSettings().CalculateTransitionDuration(Vector3.Distance(start.Position, target.Position))
                    : 0f;
                if ((!Application.isPlaying && !UsesManualUpdates) || duration <= 0f)
                {
                    target.ApplyTo(camera);
                    DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
                    continue;
                }

                var segment = AnimatePose(start, target, bounds, bounds.center, duration, false);
                while (segment.MoveNext()) yield return segment.Current;
            }

            if (waypoints.Length > 0)
            {
                MoveCompleted?.Invoke(waypoints[waypoints.Length - 1]);
            }

            activeMove = null;
        }

        private void StopActiveMove(bool notify)
        {
            if (!IsMoving)
            {
                return;
            }

            if (activeMove != null) StopCoroutine(activeMove);
            activeMove = null;
            (manualMove as IDisposable)?.Dispose();
            manualMove = null;
            if (notify)
            {
                MoveCanceled?.Invoke();
            }
        }

        private void StartMove(IEnumerator routine)
        {
            if (UsesManualUpdates) manualMove = routine;
            else activeMove = StartCoroutine(routine);
        }

        private void OnDisable() => StopActiveMove(true);

        private Camera ResolveCamera()
        {
            targetCamera = targetCamera != null ? targetCamera : Camera.main;
            return targetCamera;
        }

        private DeucarianCameraMotionSettings ResolveMotionSettings()
        {
            if (motionSettings == null)
            {
                motionSettings = DeucarianCameraMotionSettings.CreateRuntimeDefault();
            }

            return motionSettings;
        }
    }
}

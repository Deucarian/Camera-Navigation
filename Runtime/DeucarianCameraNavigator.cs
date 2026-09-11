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
        private uint moveGeneration;
        private CameraMoveOperation activeOperation;
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

        public bool IsMoving => activeMove != null;

        public void CaptureOrigin()
        {
            originPose = DeucarianCameraPose.Capture(ResolveCamera());
            hasOriginPose = true;
        }

        public bool MoveToPose(DeucarianCameraPose targetPose, Bounds bounds, Vector3 pivot, bool animate = true)
            => BeginMoveToPose(targetPose, bounds, pivot, animate, null);

        public System.Threading.Tasks.Task<CameraMoveResult> MoveToPoseAsync(DeucarianCameraPose targetPose,
            Bounds bounds, Vector3 pivot, bool animate = true, System.Threading.CancellationToken cancellationToken = default) =>
            CameraMoveOperation.Run(operation => BeginMoveToPose(targetPose, bounds, pivot, animate, operation),
                operation => { if (this != null && ReferenceEquals(activeOperation, operation)) CancelMove(); }, cancellationToken);

        private bool BeginMoveToPose(DeucarianCameraPose targetPose, Bounds bounds, Vector3 pivot,
            bool animate, CameraMoveOperation operation)
        {
            Camera camera = ResolveCamera();
            if (camera == null || !isActiveAndEnabled)
            {
                return false;
            }

            uint generation = moveGeneration + 1;
            StopActiveMove(true);
            if (generation != moveGeneration)
            {
                operation?.Complete(CameraMoveResult.Cancelled);
                return false;
            }
            activeOperation = operation;
            DeucarianCameraMotionSettings settings = ResolveMotionSettings();
            DeucarianCameraPose start = DeucarianCameraPose.Capture(camera);
            float duration = animate
                ? settings.CalculateTransitionDuration(Vector3.Distance(start.Position, targetPose.Position))
                : 0f;
            if (!Application.isPlaying || duration <= 0f)
            {
                targetPose.ApplyTo(camera);
                DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
                CompleteOperation(CameraMoveResult.Completed);
                MoveCompleted?.Invoke(targetPose);
                return true;
            }

            activeMove = StartCoroutine(AnimatePose(start, targetPose, bounds, duration));
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
            if (!isActiveAndEnabled) return false;

            uint generation = moveGeneration + 1;
            StopActiveMove(true);
            if (generation != moveGeneration) return false;
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return false;
            }

            // A non-animated routine can finish synchronously, including its callbacks.
            Coroutine move = StartCoroutine(AnimateWaypoints(waypoints, bounds, animate, generation));
            if (generation == moveGeneration) activeMove = move;
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
            float duration,
            bool completeAtEnd = true)
        {
            Camera camera = ResolveCamera();
            float elapsed = 0f;
            while (camera != null && elapsed < duration)
            {
                float movement = ResolveMotionSettings().EvaluateMovement(elapsed / duration);
                float rotation = ResolveMotionSettings().EvaluateRotation(elapsed / duration);
                DeucarianCameraPose current = new DeucarianCameraPose(
                    Vector3.LerpUnclamped(start.Position, target.Position, movement),
                    Quaternion.Slerp(start.Rotation, target.Rotation, rotation),
                    start.Orthographic,
                    Mathf.Lerp(start.OrthographicSize, target.OrthographicSize, movement),
                    Mathf.Lerp(start.FieldOfView, target.FieldOfView, movement));
                current.ApplyTo(camera);
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (camera != null)
            {
                target.ApplyTo(camera);
                DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
                if (completeAtEnd)
                {
                    activeMove = null;
                    CompleteOperation(CameraMoveResult.Completed);
                    MoveCompleted?.Invoke(target);
                }
            }

            if (completeAtEnd && camera == null)
            {
                activeMove = null;
                CompleteOperation(CameraMoveResult.InvalidTarget);
            }
        }

        private IEnumerator AnimateWaypoints(DeucarianCameraPose[] waypoints, Bounds bounds, bool animate, uint generation)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                Camera camera = ResolveCamera();
                if (camera == null)
                {
                    activeMove = null;
                    moveGeneration++;
                    yield break;
                }

                DeucarianCameraPose start = DeucarianCameraPose.Capture(camera);
                DeucarianCameraPose target = waypoints[i];
                float duration = animate
                    ? ResolveMotionSettings().CalculateTransitionDuration(Vector3.Distance(start.Position, target.Position))
                    : 0f;
                if (!Application.isPlaying || duration <= 0f)
                {
                    target.ApplyTo(camera);
                    DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
                    continue;
                }

                yield return AnimatePose(start, target, bounds, duration, false);
            }

            if (generation != moveGeneration) yield break;
            activeMove = null;
            moveGeneration++;
            if (waypoints.Length > 0)
            {
                MoveCompleted?.Invoke(waypoints[waypoints.Length - 1]);
            }

        }

        private void StopActiveMove(bool notify)
        {
            moveGeneration++;
            if (activeMove == null && activeOperation == null)
            {
                return;
            }

            if (activeMove != null) StopCoroutine(activeMove);
            activeMove = null;
            CompleteOperation(CameraMoveResult.Cancelled);
            if (notify)
            {
                MoveCanceled?.Invoke();
            }
        }

        private Camera ResolveCamera()
        {
            targetCamera = targetCamera != null ? targetCamera : Camera.main;
            return targetCamera;
        }

        private void CompleteOperation(CameraMoveResult result)
        {
            var operation = activeOperation;
            activeOperation = null;
            operation?.Complete(result);
        }

        private void OnDisable() => StopActiveMove(true);

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

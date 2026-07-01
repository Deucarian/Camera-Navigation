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
            if (!Application.isPlaying || duration <= 0f)
            {
                targetPose.ApplyTo(camera);
                DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
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

            StopActiveMove(false);
            Camera camera = ResolveCamera();
            if (camera == null)
            {
                return false;
            }

            activeMove = StartCoroutine(AnimateWaypoints(waypoints, bounds, animate));
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
                if (!Application.isPlaying || duration <= 0f)
                {
                    target.ApplyTo(camera);
                    DeucarianCameraFraming.ConfigureClipPlanes(camera, bounds);
                    continue;
                }

                yield return AnimatePose(start, target, bounds, duration, false);
            }

            if (waypoints.Length > 0)
            {
                MoveCompleted?.Invoke(waypoints[waypoints.Length - 1]);
            }

            activeMove = null;
        }

        private void StopActiveMove(bool notify)
        {
            if (activeMove == null)
            {
                return;
            }

            StopCoroutine(activeMove);
            activeMove = null;
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

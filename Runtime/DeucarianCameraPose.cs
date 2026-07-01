using System;
using UnityEngine;

namespace Deucarian.CameraNavigation
{
    [Serializable]
    public readonly struct DeucarianCameraPose
    {
        private const float MinimumFieldOfView = 0.01f;

        public DeucarianCameraPose(
            Vector3 position,
            Quaternion rotation,
            bool orthographic,
            float orthographicSize,
            float fieldOfView)
        {
            Position = position;
            Rotation = rotation == default ? Quaternion.identity : rotation;
            Orthographic = orthographic;
            OrthographicSize = Mathf.Max(0.01f, orthographicSize);
            FieldOfView = Mathf.Clamp(fieldOfView, MinimumFieldOfView, 179f);
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
        public bool Orthographic { get; }
        public float OrthographicSize { get; }
        public float FieldOfView { get; }

        public static DeucarianCameraPose Capture(Camera camera)
        {
            if (camera == null)
            {
                return new DeucarianCameraPose(Vector3.zero, Quaternion.identity, false, 10f, 60f);
            }

            return new DeucarianCameraPose(
                camera.transform.position,
                camera.transform.rotation,
                camera.orthographic,
                camera.orthographicSize,
                camera.fieldOfView);
        }

        public void ApplyTo(Camera camera)
        {
            if (camera == null)
            {
                return;
            }

            camera.orthographic = Orthographic;
            camera.orthographicSize = OrthographicSize;
            camera.fieldOfView = FieldOfView;
            camera.transform.SetPositionAndRotation(Position, Rotation);
        }

        public static DeucarianCameraPose Lerp(
            DeucarianCameraPose from,
            DeucarianCameraPose to,
            float progress,
            bool commitTargetProjection)
        {
            float t = Mathf.Clamp01(progress);
            return new DeucarianCameraPose(
                Vector3.LerpUnclamped(from.Position, to.Position, t),
                Quaternion.Slerp(from.Rotation, to.Rotation, t),
                commitTargetProjection && t >= 1f ? to.Orthographic : from.Orthographic,
                Mathf.Lerp(from.OrthographicSize, to.OrthographicSize, t),
                Mathf.Lerp(from.FieldOfView, to.FieldOfView, t));
        }
    }
}

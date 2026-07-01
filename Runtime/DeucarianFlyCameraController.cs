using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public sealed class DeucarianFlyCameraController
    {
        public void Apply(
            Camera camera,
            DeucarianFlyCameraInput input,
            float deltaTime,
            DeucarianCameraNavigationControls controls)
        {
            if (camera == null || controls == null)
            {
                return;
            }

            float boost = input.Boost ? controls.BoostScale : 1f;
            Transform transform = camera.transform;
            if (input.Look.sqrMagnitude > 0.0001f)
            {
                Vector3 euler = transform.rotation.eulerAngles;
                euler.x -= input.Look.y * controls.FlyLookSensitivity * boost;
                euler.y += input.Look.x * controls.FlyLookSensitivity * boost;
                euler.z = 0f;
                transform.rotation = Quaternion.Euler(euler);
            }

            if (input.Move.sqrMagnitude > 0.0001f)
            {
                Vector3 movement = transform.TransformDirection(input.Move.normalized) *
                                   controls.FlyMoveSensitivity *
                                   boost *
                                   Mathf.Max(0f, deltaTime);
                transform.position += movement;
            }
        }
    }
}

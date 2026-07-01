using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public sealed class DeucarianOrbitCameraController
    {
        public Vector3 Pivot { get; private set; }

        public void SetPivot(Vector3 pivot)
        {
            Pivot = pivot;
        }

        public void Apply(
            Camera camera,
            DeucarianOrbitCameraInput input,
            float deltaTime,
            DeucarianCameraNavigationControls controls)
        {
            if (camera == null || controls == null)
            {
                return;
            }

            float boost = input.Boost ? controls.BoostScale : 1f;
            Transform transform = camera.transform;
            Vector3 offset = transform.position - Pivot;
            if (offset.sqrMagnitude <= 0.0001f)
            {
                offset = -transform.forward;
            }

            if (input.Rotate.sqrMagnitude > 0.0001f)
            {
                float yaw = input.Rotate.x * controls.OrbitRotationSensitivity * boost;
                float pitch = -input.Rotate.y * controls.OrbitRotationSensitivity * boost;
                Quaternion rotation = Quaternion.AngleAxis(yaw, Vector3.up) *
                                      Quaternion.AngleAxis(pitch, transform.right);
                offset = rotation * offset;
                transform.position = Pivot + offset;
                transform.rotation = Quaternion.LookRotation(Pivot - transform.position, Vector3.up);
            }

            if (input.Pan.sqrMagnitude > 0.0001f)
            {
                Vector3 pan = (-transform.right * input.Pan.x - transform.up * input.Pan.y) *
                              controls.OrbitPanSensitivity *
                              boost *
                              Mathf.Max(0f, deltaTime);
                Pivot += pan;
                transform.position += pan;
            }

            if (Mathf.Abs(input.Zoom) > 0.0001f)
            {
                Vector3 direction = (transform.position - Pivot).normalized;
                float distance = Vector3.Distance(transform.position, Pivot);
                distance = Mathf.Max(0.1f, distance * (1f - input.Zoom * controls.OrbitZoomSensitivity));
                transform.position = Pivot + direction * distance;
            }
        }
    }
}

using UnityEngine;

namespace Deucarian.CameraNavigation.Samples
{
    /// <summary>Small caller: Inspector references feed the package's camera movement implementation.</summary>
    public sealed class BasicNavigationDemo : MonoBehaviour
    {
        [SerializeField] private DeucarianCameraNavigator navigator;
        [SerializeField] private Renderer reference;
        [SerializeField] [Min(1)] private float viewingDistance = 7;
        private Bounds Bounds => reference != null ? reference.bounds : new Bounds(Vector3.zero, Vector3.one * 3);

        private void Start() => navigator.CaptureOrigin();

        private void OnGUI()
        {
            GUILayout.BeginArea(new Rect(24, 24, 520, 150), GUI.skin.box);
            GUILayout.Label("Camera Navigation — smooth pose and framing commands");
            GUILayout.Label("Select Navigation demo or SampleMotionSettings to inspect and change values.");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Front")) Front();
            if (GUILayout.Button("Side")) Side();
            if (GUILayout.Button("Top")) Top();
            if (GUILayout.Button("Close")) Close();
            if (GUILayout.Button("Home")) Home();
            GUILayout.EndHorizontal();
            GUILayout.Label(navigator.IsMoving ? "Moving — another command smoothly replaces this one." : "Ready");
            GUILayout.EndArea();
        }
        public void Top() => navigator.MoveToTopDown(Bounds);
        public void Home() => navigator.MoveToOrigin(Bounds);
        public void Front() => Move(Vector3.back);
        public void Side() => Move(Vector3.right);
        public void Close() => Move(new Vector3(0, 0.2f, -0.6f));

        private void Move(Vector3 direction)
        {
            var camera = navigator.TargetCamera;
            Vector3 position = Bounds.center + direction * viewingDistance;
            navigator.MoveToPose(new DeucarianCameraPose(position,
                Quaternion.LookRotation(Bounds.center - position), false, camera.orthographicSize, camera.fieldOfView),
                Bounds, Bounds.center);
        }
    }
}

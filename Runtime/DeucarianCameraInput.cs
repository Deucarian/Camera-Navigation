using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public readonly struct DeucarianOrbitCameraInput
    {
        public DeucarianOrbitCameraInput(Vector2 rotate, Vector2 pan, float zoom, bool boost)
        {
            Rotate = rotate;
            Pan = pan;
            Zoom = zoom;
            Boost = boost;
        }

        public Vector2 Rotate { get; }
        public Vector2 Pan { get; }
        public float Zoom { get; }
        public bool Boost { get; }
    }

    public readonly struct DeucarianFlyCameraInput
    {
        public DeucarianFlyCameraInput(Vector2 look, Vector3 move, bool boost)
        {
            Look = look;
            Move = move;
            Boost = boost;
        }

        public Vector2 Look { get; }
        public Vector3 Move { get; }
        public bool Boost { get; }
    }
}

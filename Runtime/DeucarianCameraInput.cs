using UnityEngine;

namespace Deucarian.CameraNavigation
{
    public readonly struct DeucarianOrbitCameraInput
    {
        public DeucarianOrbitCameraInput(Vector2 rotate, Vector2 pan, float zoom, bool boost)
            : this(Vector3.zero, rotate, pan, zoom, boost, false)
        {
        }

        public DeucarianOrbitCameraInput(
            Vector3 move,
            Vector2 rotate,
            Vector2 pan,
            float zoom,
            bool boost,
            bool slow)
        {
            Move = move;
            Rotate = rotate;
            Pan = pan;
            Zoom = zoom;
            Boost = boost;
            Slow = slow;
        }

        public Vector3 Move { get; }
        public Vector2 Rotate { get; }
        public Vector2 Pan { get; }
        public float Zoom { get; }
        public bool Boost { get; }
        public bool Slow { get; }

        public bool HasInput =>
            Move.sqrMagnitude > 0.0001f ||
            Rotate.sqrMagnitude > 0.0001f ||
            Pan.sqrMagnitude > 0.0001f ||
            Mathf.Abs(Zoom) > 0.0001f;

        public static DeucarianOrbitCameraInput None { get; } =
            new DeucarianOrbitCameraInput(Vector3.zero, Vector2.zero, Vector2.zero, 0f, false, false);
    }

    public readonly struct DeucarianFlyCameraInput
    {
        public DeucarianFlyCameraInput(Vector2 look, Vector3 move, bool boost)
            : this(look, move, 0f, boost, false)
        {
        }

        public DeucarianFlyCameraInput(
            Vector2 look,
            Vector3 move,
            float zoom,
            bool boost,
            bool slow)
        {
            Look = look;
            Move = move;
            Zoom = zoom;
            Boost = boost;
            Slow = slow;
        }

        public Vector2 Look { get; }
        public Vector3 Move { get; }
        public float Zoom { get; }
        public bool Boost { get; }
        public bool Slow { get; }

        public bool HasInput =>
            Look.sqrMagnitude > 0.0001f ||
            Move.sqrMagnitude > 0.0001f ||
            Mathf.Abs(Zoom) > 0.0001f;

        public static DeucarianFlyCameraInput None { get; } =
            new DeucarianFlyCameraInput(Vector2.zero, Vector3.zero, 0f, false, false);
    }
}

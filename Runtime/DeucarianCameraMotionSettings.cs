using Deucarian.Common;
using UnityEngine;

namespace Deucarian.CameraNavigation
{
    [CreateAssetMenu(
        fileName = "DeucarianCameraMotionSettings",
        menuName = "Deucarian/Camera Navigation/Camera Motion Settings")]
    public sealed class DeucarianCameraMotionSettings : ScriptableObject
    {
        private const float DefaultTransitionMatchFieldOfView = 0.1f;

        [SerializeField, Range(0f, 100f)] private float transitionSpeed = 20f;
        [SerializeField, Range(0f, 10f)] private float minTransitionDuration = 0.1f;
        [SerializeField, Range(0f, 10f)] private float maxTransitionDuration = 1.25f;
        [SerializeField, Range(0.1f, 30f)] private float transitionMatchFieldOfView = DefaultTransitionMatchFieldOfView;
        [SerializeField] private DeucarianEasing movementEasing = DeucarianEasing.Linear;
        [SerializeField] private DeucarianEasing rotationEasing = DeucarianEasing.Linear;

        public float TransitionSpeed => Mathf.Clamp(transitionSpeed, 0f, 100f);
        public float MinTransitionDuration => Mathf.Clamp(minTransitionDuration, 0f, 10f);
        public float MaxTransitionDuration => Mathf.Max(MinTransitionDuration, Mathf.Clamp(maxTransitionDuration, 0f, 10f));
        public float TransitionMatchFieldOfView => Mathf.Clamp(transitionMatchFieldOfView, 0.1f, 30f);
        public DeucarianEasing MovementEasing => movementEasing;
        public DeucarianEasing RotationEasing => rotationEasing;

        public static DeucarianCameraMotionSettings CreateRuntimeDefault()
        {
            DeucarianCameraMotionSettings settings = CreateInstance<DeucarianCameraMotionSettings>();
            settings.name = "Runtime Deucarian Camera Motion Settings";
            return settings;
        }

        public float CalculateTransitionDuration(float distance)
        {
            float speed = TransitionSpeed;
            if (speed <= 0f)
            {
                return 0f;
            }

            return Mathf.Clamp(Mathf.Max(0f, distance) / speed, MinTransitionDuration, MaxTransitionDuration);
        }

        public float EvaluateMovement(float progress)
        {
            return DeucarianEasingUtility.Evaluate(movementEasing, progress);
        }

        public float EvaluateRotation(float progress)
        {
            return DeucarianEasingUtility.Evaluate(rotationEasing, progress);
        }

        private void OnValidate()
        {
            transitionSpeed = Mathf.Clamp(transitionSpeed, 0f, 100f);
            minTransitionDuration = Mathf.Clamp(minTransitionDuration, 0f, 10f);
            maxTransitionDuration = Mathf.Max(minTransitionDuration, Mathf.Clamp(maxTransitionDuration, 0f, 10f));
            transitionMatchFieldOfView = Mathf.Clamp(transitionMatchFieldOfView, 0.1f, 30f);
        }
    }
}

using UnityEngine;

namespace Deucarian.CameraNavigation
{
    internal sealed class DeucarianWheelZoomSmoother
    {
        private float current;
        private float target;
        private float velocity;
        private bool hasState;

        public float Current => current;

        public void Reset(float value)
        {
            current = value;
            target = value;
            velocity = 0f;
            hasState = true;
        }

        public void MultiplyTarget(
            float currentValue,
            float zoomDelta,
            float zoomStep,
            float zoomSensitivity,
            float minValue,
            float maxValue)
        {
            EnsureInitialized(currentValue);
            float scale = 1f - zoomDelta * zoomStep * zoomSensitivity;
            target = Mathf.Clamp(target * scale, minValue, maxValue);
        }

        public void AddToTarget(float currentValue, float delta)
        {
            EnsureInitialized(currentValue);
            target += delta;
        }

        public void ClampTarget(float minValue, float maxValue)
        {
            if (hasState)
            {
                target = Mathf.Clamp(target, minValue, maxValue);
            }
        }

        public bool HasPending(float currentValue, float stopEpsilon)
        {
            return hasState &&
                   Mathf.Abs(target - currentValue) > Mathf.Max(0.000001f, stopEpsilon);
        }

        public float SmoothTowardsTarget(
            float currentValue,
            float smoothingTime,
            float stopEpsilon,
            float deltaTime)
        {
            EnsureInitialized(currentValue);
            float epsilon = Mathf.Max(0.000001f, stopEpsilon);
            if (Mathf.Abs(target - currentValue) <= epsilon)
            {
                Reset(target);
                return target;
            }

            current = Mathf.SmoothDamp(
                currentValue,
                target,
                ref velocity,
                Mathf.Max(0.01f, smoothingTime),
                Mathf.Infinity,
                Mathf.Max(0f, deltaTime));

            if (Mathf.Abs(target - current) <= epsilon)
            {
                Reset(target);
            }

            return current;
        }

        private void EnsureInitialized(float value)
        {
            if (!hasState)
            {
                Reset(value);
            }
        }
    }
}

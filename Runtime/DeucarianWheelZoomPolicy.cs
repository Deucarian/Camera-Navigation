using System;

namespace Deucarian.CameraNavigation
{
    internal static class DeucarianWheelZoomPolicy
    {
        internal static float ResolveDistance(float distance, float notches, float step,
            float sensitivity, float minimum, float maximum)
        {
            if (float.IsNaN(notches) || float.IsInfinity(notches) ||
                float.IsNaN(step) || float.IsNaN(sensitivity)) return distance;

            double strength = Math.Max(0d, (double)step * sensitivity);
            if (double.IsNaN(strength)) return distance;
            double excess = Math.Max(0d, strength - 0.12d);
            double fraction = strength <= 0.12d ? strength :
                0.12d + 0.23d * (double.IsInfinity(excess) ? 1d : excess / (1d + excess));
            // Fractional and accumulated detents compose, and reversing a detent reverses its scale.
            double exponent = Math.Max(-80d, Math.Min(80d, notches * Math.Log(1d - fraction)));
            double result = distance * Math.Exp(exponent);
            return (float)Math.Max(minimum, Math.Min(maximum, result));
        }
    }
}

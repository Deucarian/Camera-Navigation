using NUnit.Framework;
using UnityEngine;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class WheelZoomContinuityTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void DefaultDetentSettlesAtASensibleDistanceAndReverses(bool fly)
        {
            using (var rig = new ZoomRig(fly))
            {
                rig.Scroll(1);
                Assert.That(rig.Distance, Is.EqualTo(10).Within(.00001f), "An input impulse is not a teleport.");
                rig.Settle();
                Assert.That(rig.Distance, Is.InRange(6.5f, 8f));
                rig.Scroll(-1); rig.Settle();
                Assert.That(rig.Distance, Is.EqualTo(10).Within(.002f));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void FractionalAndAccumulatedDetentsUseTheSameTarget(bool fly)
        {
            using (var whole = new ZoomRig(fly))
            using (var fraction = new ZoomRig(fly))
            using (var accumulated = new ZoomRig(fly))
            {
                whole.Scroll(1);
                for (int i = 0; i < 4; i++) fraction.Scroll(.25f);
                accumulated.Scroll(2); accumulated.Scroll(-1);
                whole.Settle(); fraction.Settle(); accumulated.Settle();
                Assert.That(fraction.Distance, Is.EqualTo(whole.Distance).Within(.002f));
                Assert.That(accumulated.Distance, Is.EqualTo(whole.Distance).Within(.002f));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void OrdinaryLowerSensitivityKeepsItsTwelvePercentInwardStep(bool fly)
        {
            using (var rig = new ZoomRig(fly))
            {
                rig.Controls.GlobalSensitivity = .1f;
                rig.Scroll(1); rig.Settle();
                Assert.That(rig.Distance, Is.EqualTo(8.8f).Within(.002f));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ExtremeSensitivityRemainsFiniteRespectsTheFloorAndCanReverse(bool fly)
        {
            using (var rig = new ZoomRig(fly))
            {
                rig.Controls.GlobalSensitivity = float.MaxValue;
                rig.Scroll(float.MaxValue); rig.Settle();
                Assert.That(rig.Camera.transform.position.z, Is.LessThan(0), "Zoom must not cross the pivot.");
                Assert.That(rig.Distance, Is.EqualTo(.011f).Within(.002f));
                rig.Scroll(-1); rig.Settle();
                Assert.That(rig.Distance, Is.GreaterThan(.011f));
                rig.Scroll(-float.MaxValue); rig.Settle();
                Assert.That(float.IsNaN(rig.Distance) || float.IsInfinity(rig.Distance), Is.False);
                Assert.That(rig.Distance, Is.LessThanOrEqualTo(1000001f));
            }
        }

        private sealed class ZoomRig : System.IDisposable
        {
            private readonly bool flyMode;
            private readonly DeucarianOrbitCameraController orbit = new DeucarianOrbitCameraController();
            private readonly DeucarianFlyCameraController fly = new DeucarianFlyCameraController();
            internal readonly Camera Camera;
            internal readonly DeucarianCameraNavigationControls Controls;
            internal float Distance => Camera.transform.position.magnitude;

            internal ZoomRig(bool flyMode)
            {
                this.flyMode = flyMode;
                Camera = new GameObject("Wheel continuity test").AddComponent<Camera>();
                Camera.nearClipPlane = .01f;
                Camera.transform.position = new Vector3(0, 0, -10);
                Controls = DeucarianCameraNavigationControls.CreateRuntimeDefault();
                orbit.SetPivot(Vector3.zero);
            }

            internal void Scroll(float notches) => Apply(notches, 0);
            internal void Settle() { for (int i = 0; i < 240; i++) Apply(0, .02f); }
            private void Apply(float notches, float delta)
            {
                if (flyMode) fly.Apply(Camera, new DeucarianFlyCameraInput(Vector2.zero, Vector3.zero, notches, false, false), delta, Controls, Distance);
                else orbit.Apply(Camera, new DeucarianOrbitCameraInput(Vector2.zero, Vector2.zero, notches, false), delta, Controls);
            }
            public void Dispose() { Object.DestroyImmediate(Camera.gameObject); Object.DestroyImmediate(Controls); }
        }
    }
}

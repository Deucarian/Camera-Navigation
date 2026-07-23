using Deucarian.CameraNavigation.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class DeucarianCameraNavigationDefaultsTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void RuntimeDefaultsMatchCompleteLegacyNavigationFeel()
        {
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Assert.That(
                    controls.GlobalSensitivity,
                    Is.EqualTo(1f).Within(Tolerance));
                Assert.That(
                    controls.OrbitKeyboardPanSpeed,
                    Is.EqualTo(0.9f).Within(Tolerance));
                Assert.That(
                    controls.OrbitMousePanSpeed,
                    Is.EqualTo(0.0025f).Within(Tolerance));
                Assert.That(
                    controls.OrbitOrthographicMousePanSpeed,
                    Is.EqualTo(0.003f).Within(Tolerance));
                Assert.That(
                    controls.OrbitRotationSpeed,
                    Is.EqualTo(0.25f).Within(Tolerance));
                Assert.That(
                    controls.OrbitRotationSensitivity,
                    Is.EqualTo(10f).Within(Tolerance));
                Assert.That(
                    controls.OrbitPanSensitivity,
                    Is.EqualTo(10f).Within(Tolerance));
                Assert.That(
                    controls.OrbitZoomSensitivity,
                    Is.EqualTo(1f).Within(Tolerance));
                Assert.IsFalse(controls.AllowInfiniteVerticalOrbit);
                Assert.IsTrue(controls.InvertOrbitRotation);
                Assert.That(
                    controls.FlyMoveSpeed,
                    Is.EqualTo(8f).Within(Tolerance));
                Assert.That(
                    controls.FlyRotationSpeed,
                    Is.EqualTo(0.18f).Within(Tolerance));
                Assert.That(
                    controls.FlyLookSensitivity,
                    Is.EqualTo(10f).Within(Tolerance));
                Assert.That(
                    controls.FlyMoveSensitivity,
                    Is.EqualTo(1f).Within(Tolerance));
                Assert.That(
                    controls.FlyZoomSensitivity,
                    Is.EqualTo(1f).Within(Tolerance));
                Assert.That(
                    controls.WheelZoomStep,
                    Is.EqualTo(0.12f).Within(Tolerance));
                Assert.That(
                    controls.WheelZoomSmoothingTime,
                    Is.EqualTo(0.08f).Within(Tolerance));
                Assert.That(
                    controls.WheelZoomStopEpsilon,
                    Is.EqualTo(0.001f).Within(Tolerance));
                Assert.That(
                    controls.BoostScale,
                    Is.EqualTo(4f).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void RestoreDefaultsReappliesLegacyNavigationFeel()
        {
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                var serializedControls = new SerializedObject(controls);
                serializedControls.FindProperty("orbitRotationSensitivity").floatValue =
                    0.1f;
                serializedControls.FindProperty("flyMoveSpeed").floatValue = 100f;
                serializedControls.ApplyModifiedPropertiesWithoutUndo();

                controls.ResetToDefaults();

                Assert.That(
                    controls.OrbitRotationSensitivity,
                    Is.EqualTo(10f).Within(Tolerance));
                Assert.That(
                    controls.FlyMoveSpeed,
                    Is.EqualTo(8f).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void PackageExposesOneCategorizedNavigationSettingsMenu()
        {
            Assert.AreEqual(
                "Tools/Deucarian/Experience and Interaction/World Interaction/Camera Navigation",
                DeucarianCameraNavigationSettingsWindow.MenuPath);
        }
    }
}

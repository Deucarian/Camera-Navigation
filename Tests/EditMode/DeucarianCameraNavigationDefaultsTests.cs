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
        public void RuntimeDefaultsMatchApprovedNavigationProfile()
        {
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Assert.That(
                    controls.GlobalSensitivity,
                    Is.EqualTo(10f).Within(Tolerance));
                Assert.That(
                    controls.OrbitKeyboardPanSpeed,
                    Is.EqualTo(0.9f).Within(Tolerance));
                Assert.That(
                    controls.OrbitMousePanSpeed,
                    Is.EqualTo(0.002f).Within(Tolerance));
                Assert.That(
                    controls.OrbitOrthographicMousePanSpeed,
                    Is.EqualTo(0.003f).Within(Tolerance));
                Assert.That(
                    controls.OrbitRotationSpeed,
                    Is.EqualTo(0.35f).Within(Tolerance));
                Assert.That(
                    controls.OrbitRotationSensitivity,
                    Is.EqualTo(9f).Within(Tolerance));
                Assert.That(
                    controls.OrbitPanSensitivity,
                    Is.EqualTo(14f).Within(Tolerance));
                Assert.That(
                    controls.OrbitZoomSensitivity,
                    Is.EqualTo(100f).Within(Tolerance));
                Assert.IsFalse(controls.AllowInfiniteVerticalOrbit);
                Assert.IsTrue(controls.InvertOrbitRotation);
                Assert.That(
                    controls.OrbitMinimumDistance,
                    Is.EqualTo(0.0001f).Within(Tolerance));
                Assert.That(
                    controls.OrbitMinimumDistanceScale,
                    Is.EqualTo(0.00001f).Within(Tolerance));
                Assert.That(
                    controls.OrbitNearClipDistanceMultiplier,
                    Is.EqualTo(1.1f).Within(Tolerance));
                Assert.That(
                    controls.FlyMoveSpeed,
                    Is.EqualTo(2f).Within(Tolerance));
                Assert.That(
                    controls.FlyRotationSpeed,
                    Is.EqualTo(0.24f).Within(Tolerance));
                Assert.That(
                    controls.FlyLookSensitivity,
                    Is.EqualTo(13.5f).Within(Tolerance));
                Assert.That(
                    controls.FlyMoveSensitivity,
                    Is.EqualTo(10f).Within(Tolerance));
                Assert.That(
                    controls.FlyZoomSensitivity,
                    Is.EqualTo(100f).Within(Tolerance));
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
        public void ApprovedProfileConstantsMatchProjectTuning()
        {
            Assert.That(
                DeucarianCameraNavigationControls.DefaultGlobalSensitivity,
                Is.EqualTo(10f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultWheelZoomStep,
                Is.EqualTo(0.12f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultWheelZoomSmoothingTime,
                Is.EqualTo(0.08f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultWheelZoomStopEpsilon,
                Is.EqualTo(0.001f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultBoostScale,
                Is.EqualTo(4f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitKeyboardPanSpeed,
                Is.EqualTo(0.9f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitMousePanSpeed,
                Is.EqualTo(0.002f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitOrthographicMousePanSpeed,
                Is.EqualTo(0.003f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitRotationSpeed,
                Is.EqualTo(0.35f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitRotationSensitivity,
                Is.EqualTo(0.9f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitPanSensitivity,
                Is.EqualTo(1.4f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitZoomSensitivity,
                Is.EqualTo(10f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitMinimumDistance,
                Is.EqualTo(0.0001f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitMinimumDistanceScale,
                Is.EqualTo(0.00001f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultOrbitNearClipDistanceMultiplier,
                Is.EqualTo(1.1f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultFlyMoveSpeed,
                Is.EqualTo(2f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultFlyRotationSpeed,
                Is.EqualTo(0.24f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultFlyLookSensitivity,
                Is.EqualTo(1.35f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultFlyMoveSensitivity,
                Is.EqualTo(1f).Within(Tolerance));
            Assert.That(
                DeucarianCameraNavigationControls.DefaultFlyZoomSensitivity,
                Is.EqualTo(10f).Within(Tolerance));
        }

        [Test]
        public void GlobalSensitivityMultipliesEveryNavigationChannel()
        {
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                controls.GlobalSensitivity = 2f;

                Assert.That(
                    controls.OrbitRotationSensitivity,
                    Is.EqualTo(1.8f).Within(Tolerance));
                Assert.That(
                    controls.OrbitPanSensitivity,
                    Is.EqualTo(2.8f).Within(Tolerance));
                Assert.That(
                    controls.OrbitZoomSensitivity,
                    Is.EqualTo(20f).Within(Tolerance));
                Assert.That(
                    controls.FlyLookSensitivity,
                    Is.EqualTo(2.7f).Within(Tolerance));
                Assert.That(
                    controls.FlyMoveSensitivity,
                    Is.EqualTo(2f).Within(Tolerance));
                Assert.That(
                    controls.FlyZoomSensitivity,
                    Is.EqualTo(20f).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void RuntimeControlsExposeAdjustableGlobalSensitivity()
        {
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Assert.That(
                    controls,
                    Is.AssignableTo<IDeucarianAdjustableCameraNavigationControls>());
                Assert.That(
                    DeucarianCameraNavigationControls.CanonicalResourcesPath,
                    Is.EqualTo("Deucarian/CameraNavigationControls"));
            }
            finally
            {
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void RestoreDefaultsReappliesApprovedNavigationProfile()
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
                    controls.GlobalSensitivity,
                    Is.EqualTo(10f).Within(Tolerance));
                Assert.That(
                    controls.OrbitRotationSensitivity,
                    Is.EqualTo(9f).Within(Tolerance));
                Assert.That(
                    controls.FlyMoveSpeed,
                    Is.EqualTo(2f).Within(Tolerance));
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

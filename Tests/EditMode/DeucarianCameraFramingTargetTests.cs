using NUnit.Framework;
using UnityEngine;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class DeucarianCameraFramingTargetTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void PerspectiveFramingUsesPreferredRotationAndFitsEveryCorner()
        {
            GameObject cameraObject =
                new GameObject("Perspective Framing Camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.aspect = 16f / 9f;
                camera.fieldOfView = 52f;
                camera.nearClipPlane = 0.3f;
                camera.transform.rotation =
                    Quaternion.Euler(0f, -120f, 0f);
                Bounds bounds = new Bounds(
                    new Vector3(8f, 3f, -2f),
                    new Vector3(7f, 4f, 2f));
                Quaternion preferredRotation =
                    Quaternion.Euler(12f, 38f, 0f);
                var target = new DeucarianCameraFramingTarget(
                    bounds,
                    bounds.center,
                    preferredRotation,
                    1.2f);

                bool found =
                    DeucarianCameraFraming
                        .TryCreateCurrentProjectionFramePose(
                            target,
                            camera,
                            out DeucarianCameraPose pose);

                Assert.IsTrue(found);
                Assert.IsFalse(pose.Orthographic);
                Assert.That(
                    Quaternion.Angle(
                        pose.Rotation,
                        preferredRotation),
                    Is.LessThan(Tolerance));
                pose.ApplyTo(camera);
                AssertBoundsInsideViewport(camera, bounds);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void OrthographicFramingPreservesProjectionAndFieldOfView()
        {
            GameObject cameraObject =
                new GameObject("Orthographic Framing Camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.aspect = 4f / 3f;
                camera.fieldOfView = 47f;
                camera.orthographicSize = 2f;
                camera.nearClipPlane = 0.5f;
                Bounds bounds = new Bounds(
                    new Vector3(-4f, 6f, 9f),
                    new Vector3(5f, 8f, 3f));
                Quaternion preferredRotation =
                    Quaternion.Euler(20f, -25f, 3f);
                var target = new DeucarianCameraFramingTarget(
                    bounds,
                    bounds.center,
                    preferredRotation);

                bool found =
                    DeucarianCameraFraming
                        .TryCreateCurrentProjectionFramePose(
                            target,
                            camera,
                            out DeucarianCameraPose pose);

                Assert.IsTrue(found);
                Assert.IsTrue(pose.Orthographic);
                Assert.That(
                    pose.FieldOfView,
                    Is.EqualTo(camera.fieldOfView).Within(Tolerance));
                Assert.That(
                    Quaternion.Angle(
                        pose.Rotation,
                        preferredRotation),
                    Is.LessThan(Tolerance));
                pose.ApplyTo(camera);
                AssertBoundsInsideViewport(camera, bounds);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void FramingWithoutPreferredRotationKeepsCurrentRotation()
        {
            GameObject cameraObject =
                new GameObject("Current Rotation Framing Camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.transform.rotation =
                    Quaternion.Euler(-17f, 63f, 2f);
                Bounds bounds =
                    new Bounds(Vector3.one * 4f, Vector3.one * 3f);
                var target = new DeucarianCameraFramingTarget(
                    bounds,
                    bounds.center);

                bool found =
                    DeucarianCameraFraming
                        .TryCreateCurrentProjectionFramePose(
                            target,
                            camera,
                            out DeucarianCameraPose pose);

                Assert.IsTrue(found);
                Assert.That(
                    Quaternion.Angle(
                        pose.Rotation,
                        camera.transform.rotation),
                    Is.LessThan(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void PreserveCurrentRotationPolicyIgnoresPreferredTargetRotation()
        {
            GameObject cameraObject =
                new GameObject("Preserved Rotation Framing Camera");
            DeucarianCameraFramingSettings settings =
                DeucarianCameraFramingSettings.CreateRuntimeDefault();
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.transform.rotation =
                    Quaternion.Euler(-21f, 72f, 3f);
                settings.RotationPolicy =
                    DeucarianCameraFramingRotationPolicy
                        .PreserveCurrentCameraRotation;
                Bounds bounds =
                    new Bounds(Vector3.one * 4f, Vector3.one * 3f);
                var target = new DeucarianCameraFramingTarget(
                    bounds,
                    bounds.center,
                    Quaternion.Euler(10f, -35f, 0f));

                bool found =
                    DeucarianCameraFraming
                        .TryCreateCurrentProjectionFramePose(
                            target,
                            camera,
                            settings,
                            out DeucarianCameraPose pose);

                Assert.IsTrue(found);
                Assert.That(
                    Quaternion.Angle(
                        pose.Rotation,
                        camera.transform.rotation),
                    Is.LessThan(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(settings);
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void RuntimeFramingSettingsUseApprovedDefaults()
        {
            DeucarianCameraFramingSettings settings =
                DeucarianCameraFramingSettings.CreateRuntimeDefault();
            try
            {
                Assert.That(
                    settings.RotationPolicy,
                    Is.EqualTo(
                        DeucarianCameraFramingRotationPolicy
                            .UsePreferredTargetRotation));
                Assert.That(
                    settings.PaddingMultiplier,
                    Is.EqualTo(
                        DeucarianCameraFramingSettings
                            .DefaultPaddingMultiplier));
                Assert.That(
                    settings.NearClipClearanceMultiplier,
                    Is.EqualTo(
                        DeucarianCameraFramingSettings
                            .DefaultNearClipClearanceMultiplier));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void InvalidTargetFailsWithoutProducingAPose()
        {
            GameObject cameraObject =
                new GameObject("Invalid Framing Camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                Bounds invalidBounds = new Bounds(
                    new Vector3(float.NaN, 0f, 0f),
                    Vector3.one);
                var invalidBoundsTarget =
                    new DeucarianCameraFramingTarget(
                        invalidBounds,
                        Vector3.zero);
                var invalidRotationTarget =
                    new DeucarianCameraFramingTarget(
                        new Bounds(Vector3.zero, Vector3.one),
                        Vector3.zero,
                        (Quaternion)default);

                Assert.IsFalse(
                    DeucarianCameraFraming
                        .TryCreateCurrentProjectionFramePose(
                            invalidBoundsTarget,
                            camera,
                            out _));
                Assert.IsFalse(
                    DeucarianCameraFraming
                        .TryCreateCurrentProjectionFramePose(
                            invalidRotationTarget,
                            camera,
                            out _));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        private static void AssertBoundsInsideViewport(
            Camera camera,
            Bounds bounds)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int z = 0; z < 2; z++)
                    {
                        Vector3 corner = new Vector3(
                            x == 0 ? min.x : max.x,
                            y == 0 ? min.y : max.y,
                            z == 0 ? min.z : max.z);
                        Vector3 viewport =
                            camera.WorldToViewportPoint(corner);
                        Assert.That(
                            viewport.z,
                            Is.GreaterThan(camera.nearClipPlane));
                        Assert.That(viewport.x, Is.InRange(0f, 1f));
                        Assert.That(viewport.y, Is.InRange(0f, 1f));
                    }
                }
            }
        }
    }
}

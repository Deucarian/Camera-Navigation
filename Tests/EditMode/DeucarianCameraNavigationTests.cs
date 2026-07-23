using NUnit.Framework;
using UnityEngine;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class DeucarianCameraNavigationTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void CameraPoseCaptureAndApplyRoundTripsProjection()
        {
            GameObject cameraObject = new GameObject("Camera Navigation Test Camera");
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.transform.SetPositionAndRotation(new Vector3(1f, 2f, 3f), Quaternion.Euler(10f, 20f, 0f));
                camera.orthographic = true;
                camera.orthographicSize = 7f;
                camera.fieldOfView = 45f;

                DeucarianCameraPose pose = DeucarianCameraPose.Capture(camera);
                camera.orthographic = false;
                camera.transform.position = Vector3.zero;
                pose.ApplyTo(camera);

                Assert.IsTrue(camera.orthographic);
                Assert.That(camera.orthographicSize, Is.EqualTo(7f).Within(Tolerance));
                Assert.That(Vector3.Distance(camera.transform.position, new Vector3(1f, 2f, 3f)), Is.LessThan(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void TopDownPoseLooksDownAndUsesOrthographicProjection()
        {
            Bounds bounds = new Bounds(new Vector3(3f, 2f, -5f), new Vector3(10f, 4f, 8f));

            DeucarianCameraPose pose = DeucarianCameraFraming.CreateTopDownPose(bounds, null);

            Assert.IsTrue(pose.Orthographic);
            Assert.That(Vector3.Dot(pose.Rotation * Vector3.forward, Vector3.down), Is.GreaterThan(0.999f));
            Assert.That(pose.Position.x, Is.EqualTo(bounds.center.x).Within(Tolerance));
            Assert.That(pose.Position.z, Is.EqualTo(bounds.center.z).Within(Tolerance));
        }

        [Test]
        public void ProjectionMatchedPoseKeepsPerspectiveUntilTopDownCommit()
        {
            Vector3 pivot = Vector3.zero;
            const float matchFieldOfView = 0.1f;
            DeucarianCameraPose start = new DeucarianCameraPose(
                new Vector3(0f, 4f, -10f),
                Quaternion.LookRotation(Vector3.forward, Vector3.up),
                false,
                3f,
                60f);
            DeucarianCameraPose target = new DeucarianCameraPose(
                new Vector3(0f, 10f, 0f),
                Quaternion.LookRotation(Vector3.down, Vector3.forward),
                true,
                5f,
                60f);

            DeucarianCameraPose match = DeucarianCameraFraming.CreateProjectionMatchedPose(
                start,
                target,
                pivot,
                matchFieldOfView);
            DeucarianCameraPose commit = DeucarianCameraFraming.CreateOrthographicCommitPose(match, target);

            Assert.IsFalse(match.Orthographic);
            Assert.IsTrue(commit.Orthographic);
            Assert.That(match.FieldOfView, Is.EqualTo(matchFieldOfView).Within(Tolerance));
        }

        [Test]
        public void ProjectionMatchFieldOfViewIsClampedToViewerSafeRange()
        {
            DeucarianCameraPose topDownPose = new DeucarianCameraPose(
                new Vector3(0f, 12f, 0f),
                Quaternion.LookRotation(Vector3.down, Vector3.forward),
                true,
                4f,
                60f);

            DeucarianCameraPose tiny = DeucarianCameraFraming.CreatePerspectiveMatchPoseForOrthographicSwitch(
                topDownPose,
                Vector3.zero,
                0.001f);
            DeucarianCameraPose huge = DeucarianCameraFraming.CreatePerspectiveMatchPoseForOrthographicSwitch(
                topDownPose,
                Vector3.zero,
                120f);

            Assert.That(tiny.FieldOfView, Is.EqualTo(0.1f).Within(Tolerance));
            Assert.That(huge.FieldOfView, Is.EqualTo(30f).Within(Tolerance));
        }

        [Test]
        public void VisibleTopDownTransitionPoseUsesPerspectiveWithoutChangingTargetSize()
        {
            DeucarianCameraPose topDownPose = new DeucarianCameraPose(
                new Vector3(0f, 12f, 0f),
                Quaternion.LookRotation(Vector3.down, Vector3.forward),
                true,
                4f,
                60f);

            DeucarianCameraPose visiblePose = DeucarianCameraFraming.CreateVisibleTopDownTransitionPose(
                topDownPose,
                Vector3.zero,
                45f);

            Assert.IsFalse(visiblePose.Orthographic);
            Assert.That(visiblePose.OrthographicSize, Is.EqualTo(topDownPose.OrthographicSize).Within(Tolerance));
            Assert.That(visiblePose.FieldOfView, Is.EqualTo(30f).Within(Tolerance));
            Assert.That(Vector3.Dot(visiblePose.Rotation * Vector3.forward, Vector3.down), Is.GreaterThan(0.999f));
        }

        [Test]
        public void MotionSettingsUsesBoundedDurationAndSharedEasing()
        {
            DeucarianCameraMotionSettings settings = DeucarianCameraMotionSettings.CreateRuntimeDefault();
            try
            {
                Assert.That(settings.CalculateTransitionDuration(0.1f), Is.EqualTo(settings.MinTransitionDuration).Within(Tolerance));
                Assert.That(settings.CalculateTransitionDuration(1000f), Is.EqualTo(settings.MaxTransitionDuration).Within(Tolerance));
                Assert.That(settings.EvaluateMovement(0.5f), Is.EqualTo(0.5f).Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(settings);
            }
        }

        [Test]
        public void OrbitMinimumDistanceUsesNearClipAndReferenceScale()
        {
            GameObject cameraObject = new GameObject("Orbit Minimum Distance Camera");
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.nearClipPlane = 0.01f;
                DeucarianOrbitCameraController controller =
                    new DeucarianOrbitCameraController();

                controller.SetReferenceBounds(
                    new Bounds(Vector3.zero, Vector3.one * 0.01f));
                float smallReferenceMinimum =
                    controller.GetMinimumDistance(camera, controls);
                float expectedNearClipMinimum =
                    camera.nearClipPlane * controls.OrbitNearClipDistanceMultiplier;

                Assert.That(
                    smallReferenceMinimum,
                    Is.EqualTo(expectedNearClipMinimum).Within(Tolerance));

                camera.nearClipPlane = 0.00001f;
                Bounds largeBounds =
                    new Bounds(Vector3.zero, Vector3.one * 100000f);
                controller.SetReferenceBounds(largeBounds);
                float largeReferenceMinimum =
                    controller.GetMinimumDistance(camera, controls);
                float expectedScaleMinimum =
                    largeBounds.extents.magnitude * controls.OrbitMinimumDistanceScale;

                Assert.That(
                    largeReferenceMinimum,
                    Is.EqualTo(expectedScaleMinimum).Within(Tolerance));
                Assert.That(
                    largeReferenceMinimum,
                    Is.GreaterThan(smallReferenceMinimum * 10f));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void OrbitZoomSmoothlyReachesMinimumWithoutCrossingPivot()
        {
            GameObject cameraObject = new GameObject("Orbit Close Zoom Camera");
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.nearClipPlane = 0.01f;
                camera.transform.position = new Vector3(0f, 0f, -1f);
                camera.transform.rotation =
                    Quaternion.LookRotation(Vector3.forward, Vector3.up);
                DeucarianOrbitCameraController controller =
                    new DeucarianOrbitCameraController();
                controller.SetPivot(Vector3.zero);
                controller.SetReferenceBounds(
                    new Bounds(Vector3.zero, Vector3.one * 0.01f));
                float minimumDistance =
                    controller.GetMinimumDistance(camera, controls);
                Vector3 startingDirection = camera.transform.position.normalized;

                controller.Apply(
                    camera,
                    new DeucarianOrbitCameraInput(
                        Vector3.zero,
                        Vector2.zero,
                        Vector2.zero,
                        100f,
                        false,
                        false),
                    1f / 60f,
                    controls);
                float firstFrameDistance =
                    Vector3.Distance(camera.transform.position, controller.Pivot);

                Assert.That(firstFrameDistance, Is.LessThan(1f));
                Assert.That(firstFrameDistance, Is.GreaterThan(minimumDistance));

                RunOrbitFrames(controller, camera, controls, 240);

                float finalDistance =
                    Vector3.Distance(camera.transform.position, controller.Pivot);
                Vector3 finalDirection =
                    (camera.transform.position - controller.Pivot).normalized;
                Assert.That(
                    finalDistance,
                    Is.EqualTo(minimumDistance).Within(Tolerance));
                Assert.That(finalDistance, Is.LessThan(0.025f));
                Assert.That(
                    Vector3.Dot(startingDirection, finalDirection),
                    Is.GreaterThan(0.9999f));
                Assert.That(controller.Pivot, Is.EqualTo(Vector3.zero));
                AssertFinite(camera.transform.position);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void OrbitRotationRemainsStableAtLargeReferenceMinimum()
        {
            GameObject cameraObject = new GameObject("Orbit Stability Camera");
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Vector3 pivot = new Vector3(15f, -4f, 8f);
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.nearClipPlane = 0.00001f;
                camera.transform.position = pivot + new Vector3(0f, 0f, -10f);
                camera.transform.rotation =
                    Quaternion.LookRotation(Vector3.forward, Vector3.up);
                DeucarianOrbitCameraController controller =
                    new DeucarianOrbitCameraController();
                controller.SetPivot(pivot);
                controller.SetReferenceBounds(
                    new Bounds(pivot, Vector3.one * 100000f));
                float minimumDistance =
                    controller.GetMinimumDistance(camera, controls);

                controller.Apply(
                    camera,
                    new DeucarianOrbitCameraInput(
                        Vector3.zero,
                        Vector2.zero,
                        Vector2.zero,
                        100f,
                        false,
                        false),
                    1f / 60f,
                    controls);
                RunOrbitFrames(controller, camera, controls, 240);

                for (int i = 0; i < 100; i++)
                {
                    controller.Apply(
                        camera,
                        new DeucarianOrbitCameraInput(
                            Vector3.zero,
                            new Vector2(17f, -9f),
                            Vector2.zero,
                            100f,
                            false,
                            false),
                        1f / 60f,
                        controls);
                }

                RunOrbitFrames(controller, camera, controls, 120);

                float distance =
                    Vector3.Distance(camera.transform.position, controller.Pivot);
                Vector3 directionToPivot =
                    (controller.Pivot - camera.transform.position).normalized;
                Assert.That(controller.Pivot, Is.EqualTo(pivot));
                Assert.That(
                    distance,
                    Is.EqualTo(minimumDistance).Within(0.001f));
                Assert.That(
                    Vector3.Dot(camera.transform.forward, directionToPivot),
                    Is.GreaterThan(0.999f));
                AssertFinite(camera.transform.position);
                AssertFinite(camera.transform.rotation);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void OrthographicOrbitZoomRemainsFunctional()
        {
            GameObject cameraObject = new GameObject("Orthographic Orbit Camera");
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Camera camera = cameraObject.AddComponent<Camera>();
                camera.orthographic = true;
                camera.orthographicSize = 5f;
                camera.transform.position = new Vector3(0f, 10f, 0f);
                camera.transform.rotation =
                    Quaternion.LookRotation(Vector3.down, Vector3.forward);
                DeucarianOrbitCameraController controller =
                    new DeucarianOrbitCameraController();
                controller.SetPivot(Vector3.zero);
                Vector3 startingPosition = camera.transform.position;

                controller.Apply(
                    camera,
                    new DeucarianOrbitCameraInput(
                        Vector3.zero,
                        Vector2.zero,
                        Vector2.zero,
                        1f,
                        false,
                        false),
                    1f / 60f,
                    controls,
                    false);
                RunOrbitFrames(controller, camera, controls, 120);

                Assert.IsTrue(camera.orthographic);
                Assert.That(camera.orthographicSize, Is.LessThan(5f));
                Assert.That(camera.transform.position, Is.EqualTo(startingPosition));
                Assert.That(controller.Pivot, Is.EqualTo(Vector3.zero));
                Assert.That(float.IsNaN(camera.orthographicSize), Is.False);
                Assert.That(float.IsInfinity(camera.orthographicSize), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
                Object.DestroyImmediate(controls);
            }
        }

        [Test]
        public void FlyAndOrbitWheelZoomUseSameDistanceStep()
        {
            GameObject orbitCameraObject = new GameObject("Orbit Zoom Camera");
            GameObject flyCameraObject = new GameObject("Fly Zoom Camera");
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Camera orbitCamera = orbitCameraObject.AddComponent<Camera>();
                orbitCamera.transform.position = new Vector3(0f, 0f, -10f);
                orbitCamera.transform.rotation =
                    Quaternion.LookRotation(Vector3.forward, Vector3.up);
                DeucarianOrbitCameraController orbitController =
                    new DeucarianOrbitCameraController();
                orbitController.SetPivot(Vector3.zero);
                orbitController.Apply(
                    orbitCamera,
                    new DeucarianOrbitCameraInput(
                        Vector3.zero,
                        Vector2.zero,
                        Vector2.zero,
                        1f,
                        false,
                        false),
                    1f / 60f,
                    controls);
                RunOrbitFrames(orbitController, orbitCamera, controls, 120);

                float orbitDistanceDelta =
                    10f - Vector3.Distance(orbitCamera.transform.position, Vector3.zero);

                Camera flyCamera = flyCameraObject.AddComponent<Camera>();
                flyCamera.transform.position = new Vector3(0f, 0f, -10f);
                flyCamera.transform.rotation =
                    Quaternion.LookRotation(Vector3.forward, Vector3.up);
                DeucarianFlyCameraController flyController =
                    new DeucarianFlyCameraController();
                flyController.Apply(
                    flyCamera,
                    new DeucarianFlyCameraInput(
                        Vector2.zero,
                        Vector3.zero,
                        1f,
                        false,
                        false),
                    1f / 60f,
                    controls,
                    10f);
                RunFlyFrames(flyController, flyCamera, controls, 10f, 120);

                float flyDistanceDelta = Vector3.Distance(
                    flyCamera.transform.position,
                    new Vector3(0f, 0f, -10f));
                Assert.That(
                    flyDistanceDelta,
                    Is.EqualTo(orbitDistanceDelta).Within(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(orbitCameraObject);
                Object.DestroyImmediate(flyCameraObject);
                Object.DestroyImmediate(controls);
            }
        }

        private static void RunOrbitFrames(
            DeucarianOrbitCameraController controller,
            Camera camera,
            DeucarianCameraNavigationControls controls,
            int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                controller.Apply(
                    camera,
                    DeucarianOrbitCameraInput.None,
                    1f / 60f,
                    controls);
            }
        }

        private static void RunFlyFrames(
            DeucarianFlyCameraController controller,
            Camera camera,
            DeucarianCameraNavigationControls controls,
            float referenceDistance,
            int frameCount)
        {
            for (int i = 0; i < frameCount; i++)
            {
                controller.Apply(
                    camera,
                    DeucarianFlyCameraInput.None,
                    1f / 60f,
                    controls,
                    referenceDistance);
            }
        }

        private static void AssertFinite(Vector3 value)
        {
            Assert.That(float.IsNaN(value.x) || float.IsInfinity(value.x), Is.False);
            Assert.That(float.IsNaN(value.y) || float.IsInfinity(value.y), Is.False);
            Assert.That(float.IsNaN(value.z) || float.IsInfinity(value.z), Is.False);
        }

        private static void AssertFinite(Quaternion value)
        {
            Assert.That(float.IsNaN(value.x) || float.IsInfinity(value.x), Is.False);
            Assert.That(float.IsNaN(value.y) || float.IsInfinity(value.y), Is.False);
            Assert.That(float.IsNaN(value.z) || float.IsInfinity(value.z), Is.False);
            Assert.That(float.IsNaN(value.w) || float.IsInfinity(value.w), Is.False);
        }
    }
}

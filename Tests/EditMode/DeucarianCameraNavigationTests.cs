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
    }
}

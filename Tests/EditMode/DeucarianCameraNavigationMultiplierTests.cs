using NUnit.Framework;
using UnityEngine;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class DeucarianCameraNavigationMultiplierTests
    {
        private const float Tolerance = 0.0001f;

        [Test]
        public void BoostAndSlowScaleFlyTranslationOnlyWhenModifiersAreHeld()
        {
            GameObject normalObject = new GameObject("Normal Fly Camera");
            GameObject boostObject = new GameObject("Boost Fly Camera");
            GameObject slowObject = new GameObject("Slow Fly Camera");
            DeucarianCameraNavigationControls controls =
                DeucarianCameraNavigationControls.CreateRuntimeDefault();
            try
            {
                Camera normalCamera = normalObject.AddComponent<Camera>();
                Camera boostCamera = boostObject.AddComponent<Camera>();
                Camera slowCamera = slowObject.AddComponent<Camera>();
                DeucarianFlyCameraController normalController =
                    new DeucarianFlyCameraController();
                DeucarianFlyCameraController boostController =
                    new DeucarianFlyCameraController();
                DeucarianFlyCameraController slowController =
                    new DeucarianFlyCameraController();

                normalController.Apply(
                    normalCamera,
                    CreateForwardInput(false, false),
                    1f,
                    controls);
                boostController.Apply(
                    boostCamera,
                    CreateForwardInput(true, false),
                    1f,
                    controls);
                slowController.Apply(
                    slowCamera,
                    CreateForwardInput(false, true),
                    1f,
                    controls);

                float normalDistance = normalCamera.transform.position.z;
                Assert.That(
                    boostCamera.transform.position.z,
                    Is.EqualTo(normalDistance * controls.BoostScale)
                        .Within(Tolerance));
                Assert.That(
                    slowCamera.transform.position.z,
                    Is.EqualTo(normalDistance / controls.BoostScale)
                        .Within(Tolerance));
            }
            finally
            {
                Object.DestroyImmediate(normalObject);
                Object.DestroyImmediate(boostObject);
                Object.DestroyImmediate(slowObject);
                Object.DestroyImmediate(controls);
            }
        }

        private static DeucarianFlyCameraInput CreateForwardInput(
            bool boost,
            bool slow)
        {
            return new DeucarianFlyCameraInput(
                Vector2.zero,
                Vector3.forward,
                0f,
                boost,
                slow);
        }
    }
}

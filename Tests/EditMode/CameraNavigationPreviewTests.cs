using Deucarian.CameraNavigation.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class CameraNavigationPreviewTests
    {
        [Test]
        public void GroundGridUsesThePreviewSceneWithoutExpandingFramingAndReleasesOwnedResources()
        {
            var preview = new DeucarianCameraNavigationPreview(() => null);
            GameObject grid = null;
            foreach (var root in preview.Camera.gameObject.scene.GetRootGameObjects())
                if (root.name == "Preview Grid") grid = root;
            Assert.NotNull(grid);
            var mesh = grid.GetComponent<MeshFilter>().sharedMesh;
            var material = grid.GetComponent<MeshRenderer>().sharedMaterial;
            var cubeMaterial = preview.PreviewCube.GetComponent<MeshRenderer>().sharedMaterial;
            try
            {
                Assert.AreEqual(preview.Camera.gameObject.scene, grid.scene);
                Assert.AreEqual(Vector3.one * 2, preview.Bounds.size);
                Assert.That(mesh.bounds.size.x, Is.GreaterThan(8));
                Assert.That(mesh.bounds.center.y, Is.EqualTo(-1.02f).Within(.001f));
                Assert.That(material.shader, Is.EqualTo(cubeMaterial.shader));
                Assert.AreNotSame(material, cubeMaterial);
                Assert.AreEqual(UnityEngine.Rendering.ShadowCastingMode.Off, grid.GetComponent<MeshRenderer>().shadowCastingMode);
            }
            finally { preview.Dispose(); preview.Dispose(); }
            Assert.IsTrue(grid == null); Assert.IsTrue(mesh == null); Assert.IsTrue(material == null);
            Assert.IsTrue(cubeMaterial != null, "Borrowed pipeline materials must survive preview disposal.");
        }

        [Test]
        public void EditorPointerAndWheelUseRuntimeNormalizationUnits()
        {
            Assert.AreEqual(new Vector2(1, -2), CameraNavigationPreviewInput.NormalizePointerDelta(new Vector2(10, -20), .1f));
            Assert.AreEqual(new Vector2(2, -4), CameraNavigationPreviewInput.NormalizePointerDelta(new Vector2(10, -20), .2f));
            Assert.AreEqual(Vector2.zero, CameraNavigationPreviewInput.NormalizePointerDelta(Vector2.one, 0));
            Assert.That(CameraNavigationPreviewInput.NormalizeScroll(-3, 120), Is.EqualTo(1));
            Assert.That(CameraNavigationPreviewInput.NormalizeScroll(-3, 240), Is.EqualTo(.5f));
            Assert.That(CameraNavigationPreviewInput.NormalizeScroll(1.5f, 120), Is.EqualTo(-.5f));
        }

        [Test]
        public void PreviewCameraIsIsolatedAndDisposedWithoutDirtyingTheActiveScene()
        {
            var active = SceneManager.GetActiveScene();
            bool dirty = active.isDirty;
            Camera camera;
            var preview = new DeucarianCameraNavigationPreview(() => null);
            camera = preview.Camera;
            var cube = preview.PreviewCube;
            Assert.AreEqual("Cube", cube.GetComponent<MeshFilter>().sharedMesh.name);
            Assert.AreEqual(camera.gameObject.scene, cube.scene);
            Assert.IsNotNull(cube.GetComponent<MeshRenderer>().sharedMaterial);
            Assert.IsTrue(EditorSceneManager.IsPreviewScene(camera.gameObject.scene));
            Assert.IsFalse(camera.enabled);
            Assert.AreEqual(active, SceneManager.GetActiveScene());
            preview.Dispose(); preview.Dispose();
            Assert.IsTrue(camera == null);
            Assert.IsTrue(cube == null);
            Assert.AreEqual(dirty, active.isDirty);
        }

        [Test]
        public void FrameAndResetUseTheRuntimeTransitionWithoutInstantCameraJumps()
        {
            using (var preview = new DeucarianCameraNavigationPreview(() => null))
            {
                var original = preview.Camera.transform.position;
                preview.Frame();
                Assert.IsTrue(preview.IsMoving);
                Assert.AreEqual(original, preview.Camera.transform.position);
                for (int i = 0; i < 5; i++) preview.Update(.02f);
                var intermediate = preview.Camera.transform.position;
                Assert.Greater(Vector3.Distance(original, intermediate), .001f);
                preview.Reset();
                Assert.AreEqual(intermediate, preview.Camera.transform.position, "Retargeting must begin at the current pose.");
                for (int i = 0; i < 100; i++) preview.Update(.02f);
                Assert.That(Vector3.Distance(original, preview.Camera.transform.position), Is.LessThan(.0001f));
                Assert.IsFalse(preview.IsMoving);
            }
        }

        [Test]
        public void WheelZoomContinuesBetweenEventsAndLiveControlsAffectTheSameCamera()
        {
            var controls = ScriptableObject.CreateInstance<DeucarianCameraNavigationControls>();
            try
            {
                using (var preview = new DeucarianCameraNavigationPreview(() => controls))
                {
                    var camera = preview.Camera;
                    float before = camera.transform.position.magnitude;
                    preview.ApplyInput(new DeucarianOrbitCameraInput(Vector2.zero, Vector2.zero, 1, false), DeucarianFlyCameraInput.None, .016f);
                    float first = camera.transform.position.magnitude;
                    preview.Update(.016f);
                    float second = camera.transform.position.magnitude;
                    Assert.Less(first, before); Assert.Less(second, first);
                    Assert.Greater(first, before * .8f, "A wheel impulse should not immediately reach its target.");
                    for (int i = 0; i < 100; i++) preview.Update(.02f);
                    Assert.That(camera.transform.position.magnitude / before, Is.InRange(.65f, .8f),
                        "One normalized default detent must not settle at the pivot.");
                    using (var serialized = new SerializedObject(controls))
                    {
                        serialized.FindProperty("flyMoveSpeed").floatValue = 0;
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                    preview.FlyMode = true;
                    var start = camera.transform.position;
                    var move = new DeucarianFlyCameraInput(Vector2.zero, Vector3.forward, false);
                    preview.ApplyInput(DeucarianOrbitCameraInput.None, move, .1f);
                    Assert.AreEqual(start, camera.transform.position);
                    using (var serialized = new SerializedObject(controls))
                    {
                        serialized.FindProperty("flyMoveSpeed").floatValue = 10;
                        serialized.ApplyModifiedPropertiesWithoutUndo();
                    }
                    preview.ApplyInput(DeucarianOrbitCameraInput.None, move, .1f);
                    Assert.Greater(Vector3.Distance(start, camera.transform.position), .1f);
                    Assert.AreSame(camera, preview.Camera);
                }
            }
            finally { Object.DestroyImmediate(controls); }
        }

        [Test]
        public void UserInputSupersedesFramingAndPausingStopsPendingZoom()
        {
            using (var preview = new DeucarianCameraNavigationPreview(() => null))
            {
                preview.Frame();
                preview.ApplyInput(new DeucarianOrbitCameraInput(new Vector2(10, 0), Vector2.zero, 1, false), DeucarianFlyCameraInput.None, .016f);
                Assert.IsFalse(preview.IsMoving);
                preview.StopMotion();
                var pose = DeucarianCameraPose.Capture(preview.Camera);
                for (int i = 0; i < 10; i++) preview.Update(.02f);
                Assert.AreEqual(pose.Position, preview.Camera.transform.position);
                Assert.AreEqual(pose.Rotation, preview.Camera.transform.rotation);
            }
        }

        [Test]
        public void RotationDuringFramingCancelsWithoutRestoringAnOldZoomTarget()
        {
            using (var preview = new DeucarianCameraNavigationPreview(() => null))
            {
                preview.Frame();
                for (int i = 0; i < 5; i++) preview.Update(.02f);
                Assert.IsTrue(preview.IsMoving);
                float distance = Vector3.Distance(preview.Camera.transform.position, preview.Pivot);
                preview.ApplyInput(new DeucarianOrbitCameraInput(new Vector2(.5f, 0), Vector2.zero, 0, false),
                    DeucarianFlyCameraInput.None, .02f);
                Assert.IsFalse(preview.IsMoving);
                Assert.That(Vector3.Distance(preview.Camera.transform.position, preview.Pivot), Is.EqualTo(distance).Within(.0001f));
                var afterRotation = preview.Camera.transform.position;
                for (int i = 0; i < 20; i++) preview.Update(.02f);
                Assert.That(Vector3.Distance(preview.Camera.transform.position, afterRotation), Is.LessThan(.0001f),
                    "Rotation must not resume the pre-framing zoom target.");
            }
        }

        [Test]
        public void TopDownProjectionKeepsTheTargetPlaneFramingAtTheFinalSwitch()
        {
            using (var preview = new DeucarianCameraNavigationPreview(() => null))
            {
                preview.TopDown();
                var sample = new Vector3(1, 0, 1);
                Vector3 beforeSwitch = Vector3.zero;
                for (int i = 0; i < 2000 && preview.IsMoving; i++)
                {
                    if (!preview.Camera.orthographic) beforeSwitch = preview.Camera.WorldToViewportPoint(sample);
                    preview.Update(.001f);
                }
                Assert.IsTrue(preview.Camera.orthographic);
                var afterSwitch = preview.Camera.WorldToViewportPoint(sample);
                Assert.That(Vector2.Distance(beforeSwitch, afterSwitch), Is.LessThan(.02f));
            }
        }

        [Test]
        public void ManualClockIgnoresInvalidTicksAndCancelPreventsFurtherMovement()
        {
            using (var preview = new DeucarianCameraNavigationPreview(() => null))
            {
                var navigator = preview.Camera.GetComponent<DeucarianCameraNavigator>();
                var initial = preview.Camera.transform.position;
                preview.Frame();
                navigator.Tick(0); navigator.Tick(-1); navigator.Tick(float.NaN); navigator.Tick(float.PositiveInfinity);
                Assert.AreEqual(initial, preview.Camera.transform.position);
                navigator.SetManualUpdates(false);
                Assert.IsFalse(navigator.IsMoving);
                navigator.Tick(1);
                Assert.AreEqual(initial, preview.Camera.transform.position);
            }
        }
    }
}

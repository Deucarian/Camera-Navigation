using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class CameraMoveLifecycleTests
    {
        [UnityTest]
        public IEnumerator ReplacementAndDisableCompleteOnlyTheirOwnOperation()
        {
            var go = new GameObject("camera owner");
            var camera = go.AddComponent<Camera>();
            var navigator = go.AddComponent<DeucarianCameraNavigator>();
            navigator.TargetCamera = camera;
            var settings = DeucarianCameraMotionSettings.CreateRuntimeDefault();
            navigator.MotionSettings = settings;
            using (var cancellation = new CancellationTokenSource())
            {
                try
                {
                    var bounds = new Bounds(Vector3.zero, Vector3.one * 10);
                    var pose = new DeucarianCameraPose(Vector3.one * 100, Quaternion.identity, false, 5, 60);
                    var first = navigator.MoveToPoseAsync(pose, bounds, bounds.center, true, cancellation.Token);
                    var second = navigator.MoveToPoseAsync(pose, bounds, bounds.center);
                    yield return null;
                    Assert.That(first.IsCompleted, Is.True);
                    Assert.That(first.Result, Is.EqualTo(CameraMoveResult.Cancelled));
                    cancellation.Cancel();
                    yield return null;
                    Assert.That(second.IsCompleted, Is.False, "An old token must not cancel its replacement.");
                    navigator.enabled = false;
                    yield return null;
                    Assert.That(second.Result, Is.EqualTo(CameraMoveResult.Cancelled));
                }
                finally { Object.DestroyImmediate(go); Object.DestroyImmediate(settings); }
            }
        }

        [UnityTest]
        public IEnumerator ReentrantCancellationKeepsTheNewestMove()
        {
            var go = new GameObject("reentrant camera");
            var camera = go.AddComponent<Camera>();
            var navigator = go.AddComponent<DeucarianCameraNavigator>();
            navigator.TargetCamera = camera;
            var settings = DeucarianCameraMotionSettings.CreateRuntimeDefault();
            navigator.MotionSettings = settings;
            try
            {
                var bounds = new Bounds(Vector3.zero, Vector3.one);
                var pose = new DeucarianCameraPose(Vector3.one * 100, Quaternion.identity, false, 5, 60);
                var first = navigator.MoveToPoseAsync(pose, bounds, bounds.center);
                System.Threading.Tasks.Task<CameraMoveResult> newest = null;
                System.Action callback = () => newest = navigator.MoveToPoseAsync(pose, bounds, bounds.center);
                navigator.MoveCanceled += callback;
                var superseded = navigator.MoveToPoseAsync(pose, bounds, bounds.center);
                navigator.MoveCanceled -= callback;
                yield return null;
                Assert.That(first.Result, Is.EqualTo(CameraMoveResult.Cancelled));
                Assert.That(superseded.Result, Is.EqualTo(CameraMoveResult.Cancelled));
                Assert.That(newest.IsCompleted, Is.False);
                navigator.CancelMove();
                yield return null;
                Assert.That(newest.Result, Is.EqualTo(CameraMoveResult.Cancelled));
            }
            finally { Object.DestroyImmediate(go); Object.DestroyImmediate(settings); }
        }
    }
}

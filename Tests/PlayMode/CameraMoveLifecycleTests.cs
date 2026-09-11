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
        public IEnumerator ManualCompletionKeepsAwaitableResultsAndReentrantReplacementIndependent()
        {
            var go = new GameObject("manual awaitable camera");
            var camera = go.AddComponent<Camera>();
            var navigator = go.AddComponent<DeucarianCameraNavigator>();
            var settings = DeucarianCameraMotionSettings.CreateRuntimeDefault();
            navigator.TargetCamera = camera; navigator.MotionSettings = settings;
            navigator.SetManualUpdates(true);
            try
            {
                var bounds = new Bounds(Vector3.zero, Vector3.one);
                var target = new DeucarianCameraPose(Vector3.one * 10, Quaternion.identity, false, 5, 60);
                int canceled = 0;
                navigator.MoveCanceled += () => canceled++;
                System.Threading.Tasks.Task<CameraMoveResult> replacement = null;
                System.Action<DeucarianCameraPose> completed = null;
                completed = _ =>
                {
                    navigator.MoveCompleted -= completed;
                    replacement = navigator.MoveToPoseAsync(target, bounds, bounds.center);
                };
                navigator.MoveCompleted += completed;
                var first = navigator.MoveToPoseAsync(target, bounds, bounds.center);
                yield return null; yield return null;
                Assert.That(camera.transform.position, Is.EqualTo(Vector3.zero));
                Assert.That(first.IsCompleted, Is.False);
                for (int i = 0; i < 200 && replacement == null; i++) navigator.Tick(.05f);
                for (int i = 0; i < 30 && !first.IsCompleted; i++) yield return null;
                Assert.That(first.IsCompleted, Is.True);
                Assert.That(first.Result, Is.EqualTo(CameraMoveResult.Completed));
                Assert.That(replacement, Is.Not.Null);
                Assert.That(replacement.IsCompleted, Is.False);
                Assert.That(navigator.IsMoving, Is.True);
                Assert.That(canceled, Is.Zero, "The completed manual iterator must be cleared before completion callbacks run.");
                navigator.CancelMove();
                for (int i = 0; i < 30 && !replacement.IsCompleted; i++) yield return null;
                Assert.That(replacement.IsCompleted, Is.True);
                Assert.That(replacement.Result, Is.EqualTo(CameraMoveResult.Cancelled));
            }
            finally { Object.DestroyImmediate(go); Object.DestroyImmediate(settings); }
        }

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

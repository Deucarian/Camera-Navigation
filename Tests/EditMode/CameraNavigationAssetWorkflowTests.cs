using Deucarian.CameraNavigation.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class CameraNavigationAssetWorkflowTests
    {
        private sealed class Window : EditorWindow { }
        [Test]
        public void BundledDefaultsMatchRuntimeDefaultsWithoutCreatingProjectAssets()
        {
            var controls = DeucarianCameraNavigationSettingsWindow.LoadDefaultControls();
            var framing = DeucarianCameraNavigationSettingsWindow.LoadDefaultFraming();
            var expectedControls = DeucarianCameraNavigationControls.CreateRuntimeDefault();
            var expectedFraming = DeucarianCameraFramingSettings.CreateRuntimeDefault();
            try
            {
                Assert.NotNull(controls); Assert.NotNull(framing);
                Assert.That(JsonUtility.ToJson(controls), Is.EqualTo(JsonUtility.ToJson(expectedControls)));
                Assert.That(JsonUtility.ToJson(framing), Is.EqualTo(JsonUtility.ToJson(expectedFraming)));
            }
            finally { Object.DestroyImmediate(expectedControls); Object.DestroyImmediate(expectedFraming); }
        }
    }
}

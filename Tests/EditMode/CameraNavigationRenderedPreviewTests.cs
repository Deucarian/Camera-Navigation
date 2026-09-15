using System.Collections;
using Deucarian.CameraNavigation.Editor;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class CameraNavigationRenderedPreviewTests
    {
        [UnityTest]
        public IEnumerator AttachedPreviewRendersRealGeometryAndUpdatesWithoutReplacingTheCamera()
        {
            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null) Assert.Ignore("Requires graphics.");
            var host = ScriptableObject.CreateInstance<RenderedPreviewHost>();
            var preview = new DeucarianCameraNavigationPreview(() => null);
            try
            {
                host.position = new Rect(30, 30, 640, 480);
                host.Show();
                preview.View.style.width = 600;
                preview.View.style.height = 400;
                host.rootVisualElement.Add(preview.View);
                for (int i = 0; i < 5; i++) yield return null;
                preview.RefreshView(true);
                var texture = preview.View.Q<Image>("scene-render").image;
                Assert.IsNotNull(texture);
                Assert.Greater(texture.width, 100);
                var camera = preview.Camera;
                Assert.IsTrue(preview.PreviewCube.GetComponent<MeshRenderer>().sharedMaterial.shader.isSupported);
                preview.TopDown();
                for (int i = 0; i < 100; i++) preview.Update(.02f);
                Assert.AreSame(camera, preview.Camera);
                Assert.IsTrue(camera.orthographic);
                Assert.IsNotNull(preview.View.Q<Image>("scene-render").image);
            }
            finally { preview.Dispose(); host.Close(); }
        }
        private sealed class RenderedPreviewHost : EditorWindow { }
    }
}

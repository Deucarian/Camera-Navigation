using System;
using Deucarian.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Deucarian.CameraNavigation.Editor
{
    /// <summary>Owns a real primitive and Unity's isolated preview renderer, never application objects.</summary>
    internal sealed class CameraNavigationPreviewScene : IDisposable
    {
        private readonly PreviewRenderUtility renderer;
        private readonly MeshRenderer cubeRenderer;
        private readonly Material builtInMaterial;
        private readonly CameraNavigationPreviewGrid grid;
        private bool disposed;

        internal Camera Camera => renderer.camera;
        internal GameObject Cube { get; }
        internal Bounds Bounds => cubeRenderer.bounds;

        internal CameraNavigationPreviewScene()
        {
            renderer = new PreviewRenderUtility();
            try
            {
                Camera.clearFlags = CameraClearFlags.SolidColor;
                Camera.backgroundColor = DeucarianEditorSurfacePalette.Background;
                Camera.fieldOfView = 45;
                Camera.allowHDR = false;
                renderer.ambientColor = new Color(0.35f, 0.35f, 0.35f);
                renderer.lights[0].intensity = 1.1f;
                renderer.lights[0].transform.rotation = Quaternion.Euler(40, -35, 0);
                renderer.lights[1].intensity = 0.5f;
                Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Cube.name = "Preview Cube";
                Cube.hideFlags = HideFlags.HideAndDontSave;
                Cube.transform.localScale = Vector3.one * 2;
                renderer.AddSingleGO(Cube);
                cubeRenderer = Cube.GetComponent<MeshRenderer>();
                builtInMaterial = cubeRenderer.sharedMaterial;
                grid = new CameraNavigationPreviewGrid();
                renderer.AddSingleGO(grid.Root);
                RefreshMaterial();
            }
            catch { renderer.Cleanup(); grid?.Dispose(); throw; }
        }

        private void RefreshMaterial()
        {
            var pipeline = QualitySettings.renderPipeline != null
                ? QualitySettings.renderPipeline : GraphicsSettings.defaultRenderPipeline;
            cubeRenderer.sharedMaterial = pipeline != null && pipeline.defaultMaterial != null
                ? pipeline.defaultMaterial : builtInMaterial;
            grid.RefreshMaterial(cubeRenderer.sharedMaterial);
            Camera.backgroundColor = DeucarianEditorSurfacePalette.Background;
        }

        internal Texture Render(Rect rect)
        {
            if (disposed || rect.width < 1 || rect.height < 1 || SystemInfo.graphicsDeviceType == GraphicsDeviceType.Null)
                return null;
            RefreshMaterial();
            float aspect = rect.width / rect.height;
            float width = Mathf.Min(1024, rect.width);
            rect = new Rect(0, 0, width, Mathf.Min(1024, width / aspect));
            Camera.aspect = aspect;
            renderer.BeginPreview(rect, GUIStyle.none);
            Texture texture;
            try { renderer.Render(allowScriptableRenderPipeline: true, updatefov: false); }
            finally { texture = renderer.EndPreview(); }
            return texture;
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;
            renderer.Cleanup();
            grid.Dispose();
        }
    }
}

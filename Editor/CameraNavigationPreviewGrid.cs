using System;
using System.Collections.Generic;
using Deucarian.Common;
using Deucarian.Editor;
using UnityEngine;

namespace Deucarian.CameraNavigation.Editor
{
    /// <summary>Owned preview-only ground reference; never included in target framing bounds.</summary>
    internal sealed class CameraNavigationPreviewGrid : IDisposable
    {
        private readonly Mesh mesh;
        private readonly MeshRenderer renderer;
        private Material sourceMaterial, material;
        internal GameObject Root { get; }

        internal CameraNavigationPreviewGrid()
        {
            Root = new GameObject("Preview Grid") { hideFlags = HideFlags.HideAndDontSave };
            var vertices = new List<Vector3>();
            var indices = new List<int>();
            for (int i = -4; i <= 4; i++)
            {
                AddStrip(vertices, indices, new Vector3(i, -1.02f, -4), new Vector3(i, -1.02f, 4));
                AddStrip(vertices, indices, new Vector3(-4, -1.02f, i), new Vector3(4, -1.02f, i));
            }
            mesh = new Mesh { name = "Preview Grid", hideFlags = HideFlags.HideAndDontSave };
            mesh.SetVertices(vertices); mesh.SetTriangles(indices, 0);
            var normals = new Vector3[vertices.Count];
            for (int i = 0; i < normals.Length; i++) normals[i] = Vector3.up;
            mesh.normals = normals; mesh.RecalculateBounds();
            Root.AddComponent<MeshFilter>().sharedMesh = mesh;
            renderer = Root.AddComponent<MeshRenderer>();
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        internal void RefreshMaterial(Material source)
        {
            if (source == null) return;
            if (material == null || sourceMaterial != source)
            {
                UnityObjectUtility.DestroySafely(material);
                sourceMaterial = source;
                material = new Material(source) { name = "Preview Grid", hideFlags = HideFlags.HideAndDontSave };
                renderer.sharedMaterial = material;
            }
            Color color = DeucarianEditorSurfacePalette.Border;
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
            if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        }

        private static void AddStrip(List<Vector3> vertices, List<int> indices, Vector3 start, Vector3 end)
        {
            Vector3 offset = Vector3.Cross(end - start, Vector3.up).normalized * .0125f;
            int first = vertices.Count;
            vertices.Add(start - offset); vertices.Add(start + offset);
            vertices.Add(end - offset); vertices.Add(end + offset);
            indices.AddRange(new[] { first, first + 2, first + 1, first + 2, first + 3, first + 1,
                first, first + 1, first + 2, first + 2, first + 1, first + 3 });
        }

        public void Dispose()
        {
            UnityObjectUtility.DestroySafely(Root);
            UnityObjectUtility.DestroySafely(mesh);
            UnityObjectUtility.DestroySafely(material);
            sourceMaterial = null;
        }
    }
}

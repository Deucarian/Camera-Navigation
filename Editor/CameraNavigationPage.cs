using System.Collections.Generic;
using Deucarian.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Controls = Deucarian.Editor.DeucarianEditorWorkspaceControls;

namespace Deucarian.CameraNavigation.Editor
{
    internal sealed class CameraNavigationPage
    {
        private readonly DeucarianEditorWorkspace workspace;
        private readonly List<DeucarianEditorSerializedForm> bindings = new List<DeucarianEditorSerializedForm>();
        private DeucarianCameraNavigationControls controls;
        private DeucarianCameraFramingSettings framing;
        private readonly DeucarianEditorWorkspaceForm scope;
        private int selectedTab;
        private bool playing;
        private float angle;
        private double previousTime;
        private DeucarianEditorSpatialPreview preview;
        private Button previewButton;
        public IDeucarianEditorPage Page { get; }

        internal CameraNavigationPage()
        {
            controls = DeucarianCameraNavigationSettingsWindow.FindPreferredControls();
            framing = DeucarianCameraNavigationSettingsWindow.FindPreferredFramingSettings();
            var root = new VisualElement();
            workspace = new DeucarianEditorWorkspace(root, Application.productName);
            workspace.Title.text = "Camera navigation";
            workspace.Subtitle.text = "Tune how moving through your app feels.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, DeucarianToolIds.CameraNavigation);
            workspace.SetScopeBeforeTabs();
            scope = new DeucarianEditorWorkspaceForm(workspace.Scope);
            scope.Asset("navigation-controls", "Controls asset", typeof(DeucarianCameraNavigationControls), () => controls,
                value => { controls = value as DeucarianCameraNavigationControls; Render(); });
            var tabs = new DeucarianEditorChoiceBar(new[] { "Orbit", "Fly", "Framing" }, 0);
            tabs.AddToClassList("dw-view-choices");
            tabs.Changed += value => { selectedTab = value; Render(); };
            workspace.Tabs.Add(tabs);
            Page = new DeucarianEditorPage(root, activate: _ => { scope.Refresh(); UpdatePreview(); },
                deactivate: StopPreview, update: _ => UpdatePreview(), dispose: Dispose);
            Render();
        }

        private void Render()
        {
            StopPreview();
            ClearBindings();
            workspace.Content.Clear();
            var scroll = Controls.Scroll("camera-navigation-settings");
            workspace.Content.Add(scroll);
            string title = selectedTab == 0 ? "Orbit" : selectedTab == 1 ? "Fly" : "Framing";
            string description = selectedTab == 0 ? "Rotate around a target, zoom in and out, and smooth movement."
                : selectedTab == 1 ? "Move freely and adjust how looking around feels." : "Fit a target in view without clipping its bounds.";
            var card = new DeucarianEditorFeatureSection("navigation-profile", title, description,
                selectedTab == 2 ? DeucarianEditorIconIds.Focus : DeucarianEditorIconIds.Orbit);
            scroll.Add(card.Root);
            var settings = new VisualElement();
            preview = new DeucarianEditorSpatialPreview();
            var copy = new VisualElement();
            copy.Add(card.Root.Q(className: "dw-feature-header")); copy.Add(card.Details);
            var split = Controls.Split(copy, preview);
            split.AddToClassList("dw-feature-side-preview");
            card.Root.Insert(0, split);
            card.Details.Add(settings);
            Object asset = selectedTab == 2 ? framing : controls;
            if (selectedTab == 2)
            {
                var form = new DeucarianEditorWorkspaceForm(settings);
                form.Asset("navigation-framing", "Framing asset", typeof(DeucarianCameraFramingSettings), () => framing,
                    value => { framing = value as DeucarianCameraFramingSettings; Render(); });
            }
            if (asset == null)
                settings.Add(Controls.Label("Select an asset, or create the project defaults to start tuning.", "dw-muted"));
            else
                BuildFields(settings, asset);
            if (controls == null || framing == null)
                card.Actions.Add(Controls.Button("Create project assets", CreateAssets, true));
            else
            {
                card.Actions.Add(Controls.Button("Restore defaults", Reset));
                previewButton = Controls.IconButton("Preview", DeucarianEditorIconIds.Play,
                    () => { playing = !playing; previousTime = EditorApplication.timeSinceStartup; RefreshPreviewButton(); }, DeucarianEditorButtonRole.Primary);
                card.Actions.Add(previewButton);
            }
        }

        private void BuildFields(VisualElement parent, Object asset)
        {
            var form = Bind(parent, asset);
            if (selectedTab == 0)
            {
                form.Slider("orbitRotationSpeed", "Rotation speed", 0, 2);
                form.Slider("orbitZoomSensitivity", "Zoom speed", 0.01f, 20);
                form.Slider("wheelZoomSmoothingTime", "Damping", 0.01f, 1);
                form.Property("invertOrbitRotation", "Invert vertical");
            }
            else if (selectedTab == 1)
            {
                form.Slider("flyMoveSpeed", "Move speed", 0, 20);
                form.Slider("flyRotationSpeed", "Rotation speed", 0, 2);
                form.Slider("flyLookSensitivity", "Look sensitivity", 0.01f, 10);
            }
            else { form.Remaining(); return; }
            var advanced = new Foldout { text = "Advanced settings", value = false };
            advanced.AddToClassList("dw-foldout");
            parent.Add(advanced);
            Bind(advanced, asset).Remaining(selectedTab == 0
                ? new[] { "orbitRotationSpeed", "orbitZoomSensitivity", "wheelZoomSmoothingTime", "invertOrbitRotation" }
                : new[] { "flyMoveSpeed", "flyRotationSpeed", "flyLookSensitivity" });
        }

        private DeucarianEditorSerializedForm Bind(VisualElement parent, Object asset)
        {
            var form = new DeucarianEditorSerializedForm(parent, asset);
            bindings.Add(form);
            return form;
        }

        private void CreateAssets()
        {
            controls = DeucarianCameraNavigationSettingsWindow.CreateProjectControls();
            framing = DeucarianCameraNavigationSettingsWindow.CreateProjectFramingSettings();
            scope.Refresh(); Render();
        }

        private void Reset()
        {
            Object target = selectedTab == 2 ? framing : controls;
            Undo.RecordObject(target, "Restore camera defaults");
            if (selectedTab == 2) framing.ResetToDefaults(); else controls.ResetToDefaults();
            EditorUtility.SetDirty(target);
            Render();
        }

        private void UpdatePreview()
        {
            if ((!ReferenceEquals(controls, null) && controls == null) || (!ReferenceEquals(framing, null) && framing == null))
            {
                if (controls == null) controls = null;
                if (framing == null) framing = null;
                scope.Refresh(); Render(); return;
            }
            if (!playing || preview == null) return;
            double now = EditorApplication.timeSinceStartup;
            float speed = controls == null ? 0.35f : selectedTab == 1 ? controls.FlyRotationSpeed : controls.OrbitRotationSpeed;
            angle += (float)(now - previousTime) * speed * 80;
            previousTime = now;
            preview.SetView(Quaternion.AngleAxis(22, Vector3.right) * Quaternion.AngleAxis(-32 + angle, Vector3.up));
        }

        private void StopPreview() { playing = false; RefreshPreviewButton(); }
        private void RefreshPreviewButton()
        {
            if (previewButton == null) return;
            previewButton.Q<Label>().text = playing ? "Stop preview" : "Preview";
            previewButton.tooltip = playing ? "Stop the isolated preview" : "Preview without moving your scene camera";
        }

        private void ClearBindings() { foreach (var binding in bindings) binding.Dispose(); bindings.Clear(); }
        private void Dispose() { ClearBindings(); workspace.Dispose(); }
    }
}

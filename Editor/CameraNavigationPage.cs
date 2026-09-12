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
        private double previousTime;
        private int framingRevision;
        private readonly DeucarianCameraNavigationPreview preview;
        private readonly DeucarianEditorAssetField framingPicker;
        public IDeucarianEditorPage Page { get; }

        internal CameraNavigationPage()
        {
            controls = DeucarianCameraNavigationSettingsWindow.FindPreferredControls();
            framing = DeucarianCameraNavigationSettingsWindow.FindPreferredFramingSettings();
            preview = new DeucarianCameraNavigationPreview(() => controls);
            var root = new VisualElement();
            workspace = new DeucarianEditorWorkspace(root, Application.productName);
            workspace.Title.text = "Camera navigation";
            workspace.Subtitle.text = "Tune and test camera controls in the preview.";
            DeucarianEditorWorkspaceNavigation.Populate(workspace, DeucarianToolIds.CameraNavigation);
            workspace.SetScopeBeforeTabs();
            scope = new DeucarianEditorWorkspaceForm(workspace.Scope);
            scope.AssetWithActions("navigation-controls", "Controls asset", typeof(DeucarianCameraNavigationControls), () => controls,
                value => { controls = value as DeucarianCameraNavigationControls; Render(); },
                DeucarianCameraNavigationSettingsWindow.CreateProjectControls, DeucarianEditorAssetCatalog.CopyToProject,
                DeucarianCameraNavigationSettingsWindow.LoadDefaultControls);
            framingPicker = new DeucarianEditorAssetField("navigation-framing", typeof(DeucarianCameraFramingSettings), () => framing,
                value => { framing = value as DeucarianCameraFramingSettings; Render(); },
                DeucarianCameraNavigationSettingsWindow.CreateProjectFramingSettings, DeucarianEditorAssetCatalog.CopyToProject,
                DeucarianCameraNavigationSettingsWindow.LoadDefaultFraming);
            var tabs = new DeucarianEditorChoiceBar(new[] { "Orbit", "Fly", "Framing" }, 0);
            tabs.AddToClassList("dw-view-choices");
            tabs.Changed += value => { selectedTab = value; Render(); };
            workspace.Tabs.Add(tabs);
            Page = new DeucarianEditorPage(root, activate: _ => { scope.Refresh(); previousTime = EditorApplication.timeSinceStartup; },
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
            preview.FlyMode = selectedTab == 1;
            var copy = new VisualElement();
            copy.Add(card.Root.Q(className: "dw-feature-header")); copy.Add(card.Details);
            var split = Controls.Split(copy, preview.View);
            split.AddToClassList("dw-feature-side-preview");
            card.Root.Insert(0, split);
            card.Details.Add(settings);
            Object asset = selectedTab == 2 ? framing : controls;
            if (selectedTab == 2)
            {
                var form = new DeucarianEditorWorkspaceForm(settings);
                settings.Add(Controls.Field("Framing asset", framingPicker.Root)); framingPicker.Refresh();
            }
            if (asset == null)
                settings.Add(Controls.Label("Select an asset, or create the project defaults to start tuning.", "dw-muted"));
            else
                BuildFields(settings, asset);
            if (controls == null || framing == null)
                card.Actions.Add(Controls.Button("Create project assets", CreateAssets, true));
            else
            {
                var reset = Controls.Button("Restore defaults", Reset);
                reset.SetEnabled(!DeucarianEditorAssetCatalog.IsPackageAsset(asset)); card.Actions.Add(reset);
            }
            card.Actions.Add(Controls.IconButton("Frame target", DeucarianEditorIconIds.Fit,
                () => preview.Frame(framing), DeucarianEditorButtonRole.Primary));
            card.Actions.Add(Controls.IconButton("Reset view", DeucarianEditorIconIds.Home, preview.Reset));
            if (selectedTab == 2)
                card.Actions.Add(Controls.IconButton("Top view", DeucarianEditorIconIds.Monitor, preview.TopDown));
            card.Details.Add(Controls.Label(selectedTab == 1
                ? "Drag to look · Click, then WASD + Q/E to move · Scroll to zoom"
                : "Drag to orbit · Shift-drag to pan · Scroll to zoom", "dw-muted"));
            workspace.FooterLeading.text = "Interactive preview · Your scene cameras stay unchanged";
            previousTime = EditorApplication.timeSinceStartup;
            framingRevision = framing != null ? EditorUtility.GetDirtyCount(framing) : 0;
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
            if (target == null || DeucarianEditorAssetCatalog.IsPackageAsset(target)) return;
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
            double now = EditorApplication.timeSinceStartup;
            if (selectedTab == 2 && framing != null && framingRevision != EditorUtility.GetDirtyCount(framing))
            {
                framingRevision = EditorUtility.GetDirtyCount(framing);
                preview.Frame(framing);
            }
            preview.Update((float)(now - previousTime));
            previousTime = now;
        }

        private void StopPreview() => preview.StopMotion();

        private void ClearBindings() { foreach (var binding in bindings) binding.Dispose(); bindings.Clear(); }
        private void Dispose() { ClearBindings(); preview.Dispose(); workspace.Dispose(); }
    }
}

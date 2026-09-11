using System;
using System.Collections.Generic;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.CameraNavigation.Editor
{
    [InitializeOnLoad]
    internal static class CameraNavigationControlCenterRegistration
    {
        private const string PackageId = "com.deucarian.camera-navigation";
        private static readonly IDisposable ToolRegistration;
        private static readonly IDisposable CardRegistration;

        static CameraNavigationControlCenterRegistration()
        {
            ToolRegistration = DeucarianToolRegistry.Register(
                new DeucarianToolDescriptor(
                    DeucarianToolIds.CameraNavigation,
                    "Camera Navigation",
                    "Tune and test camera controls in the preview.",
                    DeucarianControlCenterArea.Experience,
                    DeucarianCameraNavigationSettingsWindow.OpenWindow,
                    PackageId,
                    searchTerms: new[] { "camera", "orbit", "fly", "framing" },
                    order: 100, createPage: DeucarianCameraNavigationSettingsWindow.CreatePage));

            CardRegistration = DeucarianControlCenterRegistry.RegisterCardProvider(
                new CameraNavigationCardProvider());
        }

        private sealed class CameraNavigationCardProvider :
            IDeucarianControlCenterCardProvider
        {
            public string Id => PackageId + ".control-center";

            public IEnumerable<DeucarianControlCenterCard> Capture(
                DeucarianControlCenterContext context)
            {
                bool hasControls = AssetDatabase.LoadAssetAtPath<
                    DeucarianCameraNavigationControls>(
                    DeucarianCameraNavigationSettingsWindow
                        .CanonicalControlsAssetPath) != null;
                bool hasFraming = AssetDatabase.LoadAssetAtPath<
                    DeucarianCameraFramingSettings>(
                    DeucarianCameraNavigationSettingsWindow
                        .CanonicalFramingAssetPath) != null;
                bool configured = hasControls && hasFraming;

                return new[]
                {
                    new DeucarianControlCenterCard(
                        PackageId + ".setup",
                        DeucarianControlCenterArea.Experience,
                        "Camera Navigation",
                        "Project camera-navigation settings and package workflow.",
                        PackageId,
                        configured
                            ? DeucarianControlCenterStatus.Success
                            : DeucarianControlCenterStatus.Warning,
                        configured ? "Configured" : "Setup required",
                        order: 100,
                        details: new[]
                        {
                            hasControls
                                ? "Navigation controls: configured"
                                : "Navigation controls: missing",
                            hasFraming
                                ? "Framing policy: configured"
                                : "Framing policy: missing"
                        },
                        actions: new[]
                        {
                            new DeucarianControlCenterAction(
                                PackageId + ".open",
                                "Open Camera Navigation",
                                DeucarianCameraNavigationSettingsWindow.OpenWindow, navigationToolId: DeucarianToolIds.CameraNavigation)
                        },
                        searchTerms: new[] { "camera", "orbit", "fly", "framing" })
                };
            }
        }
    }
}

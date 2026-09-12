using Deucarian.Diagnostics;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Deucarian.CameraNavigation
{
    /// <summary>Typed framing requests through the configured camera navigator.</summary>
    [DisallowMultipleComponent]
    public sealed class CameraNavigationHost : MonoBehaviour, IDiagnosticProvider
    {
        private DeucarianCameraNavigator navigator;
        [SerializeField] private DeucarianCameraNavigator sceneNavigator;
        [SerializeField] private Unity.CameraPresetDefinitionCatalog definitionCatalog;
        private Dictionary<string, CameraPresetDefinition> presets;
        private readonly CancellationTokenSource lifetime = new CancellationTokenSource();
        private bool destroyed;

        public void Configure(DeucarianCameraNavigator value, IEnumerable<CameraPresetDefinition> definitions)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(CameraNavigationHost));
            if (navigator != null) throw new InvalidOperationException("CameraNavigationHost '" + name + "' is already configured.");
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (value.TargetCamera == null) throw new InvalidOperationException("Assign the navigator's TargetCamera before configuring CameraNavigationHost '" + name + "'.");
            if (definitions == null) throw new ArgumentNullException(nameof(definitions));
            var copy = new Dictionary<string, CameraPresetDefinition>(StringComparer.Ordinal);
            foreach (var definition in definitions)
            {
                if (definition == null) throw new ArgumentException("A camera preset definition is null.", nameof(definitions));
                if (copy.ContainsKey(definition.Key.Id)) throw new ArgumentException("Camera preset '" + definition.Key.Id + "' is configured twice. Register it once.", nameof(definitions));
                copy.Add(definition.Key.Id, definition);
            }
            presets = copy;
            navigator = value;
        }

        public async Task<CameraMoveResult> FrameAsync(Bounds bounds, CameraPresetKey preset, CancellationToken cancellationToken = default)
        {
            if (destroyed) throw new ObjectDisposedException(nameof(CameraNavigationHost));
            if (preset == null) throw new ArgumentNullException(nameof(preset), "Select a CameraPresetKey or reuse a named camera preset.");
            if (navigator == null) throw new InvalidOperationException("CameraNavigationHost '" + name + "' is not configured. Supply its navigator and camera preset definitions during startup.");
            if (!presets.TryGetValue(preset.Id, out var definition)) throw new InvalidOperationException("Camera preset '" + preset.Id + "' is absent from '" + name + "'. Add its CameraPresetDefinition to this host's configuration.");
            var target = new DeucarianCameraFramingTarget(bounds, bounds.center, definition.Padding, definition.DistanceProfile);
            if (!DeucarianCameraFraming.TryCreateCurrentProjectionFramePose(target, navigator.TargetCamera, definition.Framing, out var pose))
                return CameraMoveResult.InvalidTarget;
            using (var cancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetime.Token, cancellationToken))
                return await navigator.MoveToPoseAsync(pose, bounds, target.FocusPoint, definition.Animate, cancellation.Token);
        }

        private void OnDestroy() { diagnosticRegistration?.Dispose(); diagnosticRegistration = null;  destroyed = true; lifetime.Cancel(); lifetime.Dispose(); navigator = null; presets?.Clear(); }
        private DiagnosticProviderRegistration diagnosticRegistration;
        private void Awake()
        {
            diagnosticRegistration = DiagnosticProviderRegistry.Register(this);
            if (navigator == null && sceneNavigator != null)
                Configure(sceneNavigator, (definitionCatalog != null ? definitionCatalog : Unity.CameraPresetDefinitionCatalog.LoadProject()).CreateRuntimeDefinitions());
        }
        string IDiagnosticProvider.ProviderId => "camera-navigation.host." + GetInstanceID();
        string IDiagnosticProvider.DisplayName => "CameraNavigationHost";
        void IDiagnosticProvider.Collect(DiagnosticReportBuilder builder)
        {
            bool configured = navigator != null;
            builder.AddSection(((IDiagnosticProvider)this).ProviderId, "CameraNavigationHost")
                .AddItem("configured", "Configured", configured ? "Ready" : "Call Configure during startup",
                    configured ? DiagnosticSeverity.Info : DiagnosticSeverity.Warning)
                .AddItem("enabled", "Enabled", isActiveAndEnabled.ToString());
        }
    }
}

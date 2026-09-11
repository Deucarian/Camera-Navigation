using System;

using UnityEngine;

namespace Deucarian.CameraNavigation.Unity
{
    /// <summary>Reusable defaults for code calls and serialized typed keys; runtime state remains in the core service.</summary>
    public sealed class CameraPresetDefinitionAsset : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private DeucarianCameraFramingSettings framing = null;
        [SerializeField] private float padding = 1.1f;
        [SerializeField] private DeucarianCameraFramingDistanceProfile distanceProfile = DeucarianCameraFramingDistanceProfile.Standard;
        [SerializeField] private bool animate = true;
        public string Id => id;
        public string DisplayName => displayName;
        public CameraPresetKey Key => new AssetKey(id);
        public CameraPresetDefinition ToRuntimeDefinition() => new CameraPresetDefinition(Key, framing, padding, distanceProfile, animate);
        private sealed class AssetKey : CameraPresetKey { public AssetKey(string value) : base(value) { } }
    }
}

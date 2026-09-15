using System;
using UnityEngine;

namespace Deucarian.CameraNavigation
{
    /// <summary>A declared CameraPreset identity. Reuse a named definition or select it in the Inspector.</summary>
    [Serializable]
    public class CameraPresetKey : IEquatable<CameraPresetKey>
    {
        [SerializeField] private string definitionId;

        /// <summary>For central definition sets and generated declarations; ordinary callers reuse those keys.</summary>
        protected CameraPresetKey(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || id != id.Trim())
                throw new ArgumentException("A CameraPresetKey definition needs a non-empty stable ID without surrounding whitespace.", nameof(id));
            definitionId = id;
        }

        public string Id => !string.IsNullOrWhiteSpace(definitionId) ? definitionId :
            throw new InvalidOperationException("No CameraPresetKey is selected. Select an existing definition in the Inspector or assign a named key from a CameraPresetKeySet declaration.");
        public bool Equals(CameraPresetKey other) => other != null && string.Equals(definitionId, other.definitionId, StringComparison.Ordinal);
        public override bool Equals(object other) => other is CameraPresetKey key && Equals(key);
        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(definitionId ?? string.Empty);
        public override string ToString() => definitionId ?? string.Empty;
    }
}

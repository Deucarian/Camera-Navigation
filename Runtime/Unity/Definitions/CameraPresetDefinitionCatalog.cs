using System;
using System.Linq;
using UnityEngine;

namespace Deucarian.CameraNavigation.Unity
{
    public sealed class CameraPresetDefinitionCatalog : ScriptableObject
    {
        public const string ResourcePath = "Deucarian/Definitions/CameraPresetDefinitionCatalog";
        [SerializeField] private CameraPresetDefinitionAsset[] definitions = Array.Empty<CameraPresetDefinitionAsset>();
        public static CameraPresetDefinitionCatalog LoadProject() => Resources.Load<CameraPresetDefinitionCatalog>(ResourcePath) ?? throw new InvalidOperationException("Create a CameraPreset definition in Definitions before loading its project catalog.");
        public CameraPresetDefinition[] CreateRuntimeDefinitions()
        {
            if (definitions.Any(x => x == null) || definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("The CameraPreset catalog contains missing or duplicate definitions. Synchronize it in Definitions.");
            return definitions.Select(x => x.ToRuntimeDefinition()).ToArray();
        }
    }
}

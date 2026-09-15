using System;
using System.Linq;
using Deucarian.Editor.Definitions;
using Deucarian.CameraNavigation.Unity;
using UnityEditor;

namespace Deucarian.CameraNavigation.Editor.Definitions
{
    internal static class ProjectDefinitionCatalogAuthoring
    {
        internal static void Refresh(bool validateOnly = false)
        {
            var definitions = AssetDatabase.FindAssets("t:CameraPresetDefinitionAsset", new[] { "Assets" })
                .Select(x => AssetDatabase.LoadAssetAtPath<CameraPresetDefinitionAsset>(AssetDatabase.GUIDToAssetPath(x)))
                .Where(x => x != null).OrderBy(x => x.Id, StringComparer.Ordinal).ToArray();
            if (definitions.GroupBy(x => x.Id).Any(x => x.Count() > 1)) throw new InvalidOperationException("CameraPreset definition IDs must be unique.");
            DeucarianDefinitionCatalog.Update<CameraPresetDefinitionCatalog>("Assets/DeucarianDefinitions/Resources/Deucarian/Definitions/CameraPresetDefinitionCatalog.asset", "definitions", definitions, validateOnly);
        }
    }
}

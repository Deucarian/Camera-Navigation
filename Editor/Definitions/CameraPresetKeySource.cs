using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.CameraNavigation.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.CameraNavigation.Editor.Definitions
{
    public sealed class CameraPresetKeySource : DeucarianAssetKeySource<CameraPresetDefinitionAsset>
    {
        public override Type KeyType => typeof(CameraPresetKey);
        public override Type DefinitionSetAttribute => typeof(CameraPresetKeySetAttribute);
        public override string GeneratedClassName => "ProjectCameraPresets";
        protected override DeucarianKeyChoice ReadDefinition(CameraPresetDefinitionAsset asset) => new DeucarianKeyChoice(asset.Id, asset.DisplayName);
    }
}

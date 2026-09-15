using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.CameraNavigation.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.CameraNavigation.Editor.Definitions
{
    public sealed class CameraPresetDefinitionSchema : DeucarianSerializedDefinitionSchema<CameraPresetDefinitionAsset, CameraPresetDefinitionSpec>
    {
        public override string Id => "camera-presets";
        public override string DisplayName => "Camera presets";
        public override void ValidateAssetReady(ScriptableObject asset)
        {
            base.ValidateAssetReady(asset);
            ((CameraPresetDefinitionAsset)asset).ToRuntimeDefinition();
        }
        public override void RefreshCatalog(bool validateOnly = false) { ProjectDefinitionCatalogAuthoring.Refresh(validateOnly); }
        [MenuItem("Assets/Create/Deucarian/Camera Navigation/CameraPreset Definition")]
        private static void CreateDefinition() { Selection.activeObject = DeucarianDefinitionSync.Create(new CameraPresetDefinitionSchema(), "NewCameraPreset"); }
    }
}

using System;
using System.Linq;
using Deucarian.Editor;
using Deucarian.Editor.Definitions;
using Deucarian.CameraNavigation.Unity;
using UnityEditor;
using UnityEngine;

namespace Deucarian.CameraNavigation.Editor.Definitions
{
    [Serializable]
    public sealed class CameraPresetDefinitionSpec : DeucarianDefinitionSpec
    {
        [DefinitionField("framing")] public DeucarianCameraFramingSettings Framing = null;
        [DefinitionField("padding")] public float Padding = 1.1f;
        [DefinitionField("distanceProfile")] public DeucarianCameraFramingDistanceProfile DistanceProfile = DeucarianCameraFramingDistanceProfile.Standard;
        [DefinitionField("animate")] public bool Animate = true;
    }
}

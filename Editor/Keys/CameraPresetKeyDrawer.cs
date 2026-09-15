using System;
using Deucarian.Editor;
using UnityEditor;

namespace Deucarian.CameraNavigation.Editor
{
    [CustomPropertyDrawer(typeof(CameraPresetKey), true)]
    public sealed class CameraPresetKeyDrawer : DeucarianKeyDrawer
    {
        public override Type KeyType => typeof(CameraPresetKey);
        public override Type DefinitionSetAttribute => typeof(CameraPresetKeySetAttribute);
        public override string SetupHint => "Select an existing CameraPresetKey; declare reusable keys once in a [CameraPresetKeySet] class.";
    }
}

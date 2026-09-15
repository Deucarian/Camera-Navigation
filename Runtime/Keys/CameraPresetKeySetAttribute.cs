using System;

namespace Deucarian.CameraNavigation
{
    /// <summary>Marks an authoritative set of named CameraPresetKey fields or properties for the Inspector.</summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class CameraPresetKeySetAttribute : Attribute { }
}

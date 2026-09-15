namespace Deucarian.CameraNavigation.Samples.SimpleUsage
{
    [CameraPresetKeySet]
    public static class CameraPresets
    {
        public static CameraPresetKey Overview => new Definition();
        private sealed class Definition : CameraPresetKey
        {
            public Definition() : base("sample.overview") { }
        }
    }
}

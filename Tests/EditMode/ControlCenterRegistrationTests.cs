using System.Linq;
using Deucarian.Editor;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class ControlCenterRegistrationTests
    {
        private const string PackageId =
            "com.deucarian.camera-navigation";

        [Test]
        public void ReturningToThePagePreservesExpandedSettings()
        {
            var controls = ScriptableObject.CreateInstance<DeucarianCameraNavigationControls>();
            try
            {
                Assert.IsTrue(DeucarianToolRegistry.TryGet(DeucarianToolIds.CameraNavigation, out var tool));
                using (var page = tool.CreatePage())
                {
                    page.Root.Q<ObjectField>("navigation-controls").value = controls;
                    var advanced = page.Root.Query<Foldout>().ToList().Single(f => f.text == "Advanced settings");
                    advanced.value = true;
                    page.Deactivate(); page.Activate(null);
                    Assert.IsTrue(page.Root.Contains(advanced));
                    Assert.IsTrue(advanced.value);
                    Object.DestroyImmediate(controls);
                    Assert.DoesNotThrow(() => page.Activate(null));
                }
            }
            finally { if (controls != null) Object.DestroyImmediate(controls); }
        }

        [Test]
        public void PackageRegistersStableToolAndCard()
        {
            Assert.That(
                DeucarianToolRegistry.TryGet(
                    DeucarianToolIds.CameraNavigation,
                    out DeucarianToolDescriptor tool),
                Is.True);
            Assert.That(tool.OwningPackage, Is.EqualTo(PackageId));

            DeucarianControlCenterSnapshot snapshot =
                DeucarianControlCenterSnapshotBuilder.Capture(true);
            Assert.That(
                snapshot.Cards.Any(
                    card => card.OwningPackage == PackageId),
                Is.True);
        }
    }
}

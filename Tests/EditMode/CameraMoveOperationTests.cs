using System.Threading;
using NUnit.Framework;

namespace Deucarian.CameraNavigation.Tests
{
    public sealed class CameraMoveOperationTests
    {
        [Test]
        public void PrecancelledMoveDoesNotStartAndImmediateCompletionIsObservable()
        {
            var cancelled = new CancellationToken(true);
            Assert.That(CameraMoveOperation.Run(_ => throw new System.Exception("must not start"), _ => { }, cancelled).Result,
                Is.EqualTo(CameraMoveResult.Cancelled));
            Assert.That(CameraMoveOperation.Run(operation => { operation.Complete(CameraMoveResult.Completed); return true; },
                _ => { }, CancellationToken.None).Result, Is.EqualTo(CameraMoveResult.Completed));
            Assert.That(CameraMoveOperation.Run(_ => false, _ => { }, CancellationToken.None).Result,
                Is.EqualTo(CameraMoveResult.InvalidTarget));
        }
    }
}

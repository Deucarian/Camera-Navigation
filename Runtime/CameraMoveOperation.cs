using System;
using System.Threading;
using System.Threading.Tasks;

namespace Deucarian.CameraNavigation
{
    public enum CameraMoveResult { Completed, Cancelled, InvalidTarget }

    /// <summary>Completion adapter for a camera owner's existing motion. It never moves a camera itself.</summary>
    public sealed class CameraMoveOperation
    {
        private readonly TaskCompletionSource<CameraMoveResult> completion =
            new TaskCompletionSource<CameraMoveResult>(TaskCreationOptions.RunContinuationsAsynchronously);

        public void Complete(CameraMoveResult result) => completion.TrySetResult(result);

        /// <summary>Called on Unity's main thread; cancellation is posted back to that thread before stopping motion.</summary>
        public static async Task<CameraMoveResult> Run(Func<CameraMoveOperation, bool> start,
            Action<CameraMoveOperation> cancel, CancellationToken cancellationToken)
        {
            if (start == null) throw new ArgumentNullException(nameof(start));
            if (cancel == null) throw new ArgumentNullException(nameof(cancel));
            if (cancellationToken.IsCancellationRequested) return CameraMoveResult.Cancelled;
            var context = SynchronizationContext.Current;
            if (cancellationToken.CanBeCanceled && context == null)
                throw new InvalidOperationException("Start cancellable camera moves on Unity's main thread so cancellation can safely stop the camera.");
            var operation = new CameraMoveOperation();
            using (cancellationToken.Register(() => context.Post(_ =>
            {
                if (operation.completion.Task.IsCompleted) return;
                cancel(operation);
                operation.Complete(CameraMoveResult.Cancelled);
            }, null)))
            {
                try
                {
                    if (!start(operation)) operation.Complete(CameraMoveResult.InvalidTarget);
                    return await operation.completion.Task;
                }
                catch { cancel(operation); throw; }
            }
        }
    }
}

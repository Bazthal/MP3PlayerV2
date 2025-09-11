using System.Threading.Channels;

namespace MP3PlayerV2.Services
{
    public static class LiteDbWriteQueue
    {
        private static readonly Channel<Action> _queue = Channel.CreateUnbounded<Action>();
        private static readonly CancellationTokenSource _cts = new();
        private static int _inProgress;
        private static TaskCompletionSource<bool> _emptyTcs = CreateNewTcs();

        static LiteDbWriteQueue()
        {
            Task.Factory.StartNew(() => ProcessQueue(_cts.Token), TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Creates a new <see cref="TaskCompletionSource{TResult}"/> configured to run continuations asynchronously.
        /// </summary>
        /// <remarks>The <see cref="TaskCompletionSource{TResult}"/> is created with the <see
        /// cref="TaskCreationOptions.RunContinuationsAsynchronously"/> option, ensuring that continuations added to the
        /// associated task are executed asynchronously.</remarks>
        /// <returns>A new instance of <see cref="TaskCompletionSource{TResult}"/> with a result type of <see cref="bool"/>.</returns>
        private static TaskCompletionSource<bool> CreateNewTcs() =>
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        /// <summary>
        /// Processes actions from the queue asynchronously until the queue is empty or the operation is canceled.
        /// </summary>
        /// <remarks>This method reads actions from the queue and executes them sequentially. If an
        /// exception occurs during the execution of an action,  it is logged, and processing continues with the next
        /// action. The method ensures that the queue's state is updated appropriately  after each action is processed.
        /// When the queue becomes empty, a task completion source is signaled.</remarks>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the operation.</param>
        /// <returns></returns>
        private static async Task ProcessQueue(CancellationToken cancellationToken)
        {
            await foreach (var action in _queue.Reader.ReadAllAsync(cancellationToken))
            {
                try
                {
                    Interlocked.Increment(ref _inProgress);
                    action();
                }
                catch (Exception ex)
                {
                    BazthalLib.DebugUtils.Log("LiteDbWriteQueue", "Exception", ex.ToString());
                }
                finally
                {
                    if (Interlocked.Decrement(ref _inProgress) == 0 && _queue.Reader.Count == 0)
                        _emptyTcs.TrySetResult(true);
                }
            }
        }

        /// <summary>
        /// Enqueues an action to be executed, with an optional completion callback and cancellation support.
        /// </summary>
        /// <param name="action">The action to execute.</param>
        /// <param name="afterComplete">An optional callback to invoke after the action completes.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        public static void Enqueue(Action action, Action? afterComplete = null, CancellationToken cancellationToken = default)
        {
            _queue.Writer.TryWrite(() =>
            {
                try
                {
                    if (!cancellationToken.IsCancellationRequested)
                        action();
                }
                finally
                {
                    afterComplete?.Invoke();
                }
            });

            if (_emptyTcs.Task.IsCompleted)
                _emptyTcs = CreateNewTcs();
        }

        /// <summary>
        /// Asynchronously waits until the associated operation or state is completed and becomes empty.
        /// </summary>
        /// <remarks>This method returns a task that completes when the associated operation or state
        /// transitions to an empty state. It can be used to await the completion of processes that signal their
        /// readiness through this mechanism.</remarks>
        /// <returns>A task that represents the asynchronous operation. The task completes when the associated state becomes
        /// empty.</returns>
        public static Task WaitForEmptyAsync() => _emptyTcs.Task;

        /// <summary>
        /// Clears all items from the queue.
        /// </summary>
        /// <remarks>This method removes all items currently in the queue. It is a no-op if the queue is
        /// already empty.</remarks>
        public static void Clear()
        {
            while (_queue.Reader.TryRead(out _)) { }
        }

        /// <summary>
        /// Shuts down the system by completing the message queue and canceling any ongoing operations.
        /// </summary>
        /// <remarks>This method signals the completion of the message queue and cancels any pending
        /// operations  associated with the system. Once called, no further messages can be written to the queue,  and
        /// any ongoing operations relying on the associated cancellation token will be terminated.</remarks>
        public static void Shutdown()
        {
            _queue.Writer.TryComplete();
            _cts.Cancel();
        }

    }
}

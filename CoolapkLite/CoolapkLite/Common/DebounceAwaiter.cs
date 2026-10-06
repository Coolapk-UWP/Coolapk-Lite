using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoolapkLite.Common
{
    public sealed class DebounceAwaiter : IThreadSwitcher<DebounceAwaiter>
    {
        private readonly int _timeout;
        private readonly TaskScheduler _scheduler = SynchronizationContext.Current != null ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Default;

        private Action _continuation;
        private CancellationTokenSource _cancellationTokenSource;

        public DebounceAwaiter(int timeout) => _timeout = timeout;

        public bool IsCompleted => false;

        public void GetResult() { }

        public DebounceAwaiter GetAwaiter() => this;

        IThreadSwitcher IThreadSwitcher.GetAwaiter() => GetAwaiter();

        public void OnCompleted(Action continuation)
        {
            if (_continuation == null) { _continuation = continuation; }
            SetTimeout();
        }

        private void SetTimeout()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            CancellationTokenSource cts = _cancellationTokenSource = new CancellationTokenSource();
            Task.Delay(_timeout, cts.Token).ContinueWith(
                _ => InvokeContinuation(cts),
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnRanToCompletion,
                _scheduler);
        }

        private void InvokeContinuation(CancellationTokenSource cts)
        {
            if (cts != _cancellationTokenSource) { return; }
            cts.Dispose();
            _cancellationTokenSource = null;
            Action continuation = _continuation;
            _continuation = null;
            continuation();
        }
    }
}

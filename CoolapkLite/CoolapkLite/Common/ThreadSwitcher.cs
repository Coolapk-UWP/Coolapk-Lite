using CoolapkLite.Helpers;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using ThreadPool = Windows.System.Threading.ThreadPool;

namespace CoolapkLite.Common
{
    public readonly struct DispatcherThreadSwitcher : INotifyCompletion
    {
        private readonly CoreDispatcher dispatcher;

        public bool IsCompleted => dispatcher.HasThreadAccess;

        internal DispatcherThreadSwitcher(CoreDispatcher dispatcher) => this.dispatcher = dispatcher;

        public void GetResult() { }

        public DispatcherThreadSwitcher GetAwaiter() => this;

        public void OnCompleted(Action continuation)
        {
            void test()
            {
                try
                {
                    continuation();
                }
                catch(Exception ex)
                {
                }
            }
            if (IsCompleted)
            {
                test();
            }
            else
            {
                _ = dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => test());
            }
        }
    }

    public readonly struct DispatcherQueueThreadSwitcher : INotifyCompletion
    {
        private readonly DispatcherQueue dispatcher;

        public bool IsCompleted => ThreadSwitcher.IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess;

        internal DispatcherQueueThreadSwitcher(DispatcherQueue dispatcher) => this.dispatcher = dispatcher;

        public void GetResult() { }

        public DispatcherQueueThreadSwitcher GetAwaiter() => this;

        public void OnCompleted(Action continuation)
        {
            if (IsCompleted)
            {
                continuation();
            }
            else
            {
                _ = dispatcher.TryEnqueue(DispatcherQueuePriority.Normal, () => continuation());
            }
        }
    }

    public readonly struct ThreadPoolThreadSwitcher : INotifyCompletion
    {
        public bool IsCompleted => SynchronizationContext.Current == null;

        public void GetResult() { }

        public ThreadPoolThreadSwitcher GetAwaiter() => this;

        public void OnCompleted(Action continuation)
        {
            void test()
            {
                try
                {
                    continuation();
                }
                catch (Exception ex)
                {
                }
            }
            _ = ThreadPool.RunAsync(_ => test(), WorkItemPriority.Normal);
        }
    }

    public static class ThreadSwitcher
    {
        public static bool IsHasThreadAccessPropertyAvailable { get; } = ApiInfoHelper.IsHasThreadAccessSupported;

        public static DispatcherQueueThreadSwitcher ResumeForegroundAsync(this DispatcherQueue dispatcher) => new DispatcherQueueThreadSwitcher(dispatcher);

        public static DispatcherThreadSwitcher ResumeForegroundAsync(this CoreDispatcher dispatcher) => new DispatcherThreadSwitcher(dispatcher);

        public static ThreadPoolThreadSwitcher ResumeBackgroundAsync() => new ThreadPoolThreadSwitcher();
    }
}
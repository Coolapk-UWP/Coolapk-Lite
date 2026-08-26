using CoolapkLite.Helpers;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using ThreadPool = Windows.System.Threading.ThreadPool;

namespace CoolapkLite.Common
{
    /// <summary>
    /// The interface of helper type for switch thread.
    /// </summary>
    public interface IThreadSwitcher : INotifyCompletion
    {
        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        bool IsCompleted { get; }

        /// <summary>
        /// Ends the await on the completed task.
        /// </summary>
        void GetResult();

        /// <summary>
        /// Gets an awaiter used to await this <see cref="IThreadSwitcher"/>.
        /// </summary>
        /// <returns>An awaiter instance.</returns>
        IThreadSwitcher GetAwaiter();
    }

    /// <summary>
    /// The interface of helper type for switch thread.
    /// </summary>
    /// <typeparam name="T">The type of the result of <see cref="GetAwaiter"/>.</typeparam>
    public interface IThreadSwitcher<out T> : IThreadSwitcher
    {
        /// <summary>
        /// Gets an awaiter used to await <typeparamref name="T"/>.
        /// </summary>
        /// <returns>A <typeparamref name="T"/> awaiter instance.</returns>
        new T GetAwaiter();
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="CoreDispatcher"/>. This type is not intended to be used directly from your code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct CoreDispatcherThreadSwitcher : IThreadSwitcher<CoreDispatcherThreadSwitcher>
    {
        /// <summary>
        /// A <see cref="CoreDispatcher"/> whose foreground thread to switch execution to.
        /// </summary>
        private readonly CoreDispatcher dispatcher;

        /// <summary>
        /// Specifies the priority for event dispatch.
        /// </summary>
        private readonly CoreDispatcherPriority priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreDispatcherThreadSwitcher"/> struct.
        /// </summary>
        /// <param name="dispatcher">A <see cref="CoreDispatcher"/> whose foreground thread to switch execution to.</param>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        public CoreDispatcherThreadSwitcher(CoreDispatcher dispatcher, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            this.dispatcher = dispatcher;
            this.priority = priority;
        }

        /// <inheritdoc/>
        public bool IsCompleted => dispatcher?.HasThreadAccess != false;

        /// <inheritdoc/>
        public void GetResult() { }

        /// <inheritdoc/>
        public CoreDispatcherThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = dispatcher.RunAsync(priority, continuation.Invoke);
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="DispatcherQueue"/>. This type is not intended to be used directly from your code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct DispatcherQueueThreadSwitcher : IThreadSwitcher<DispatcherQueueThreadSwitcher>
    {
        /// <summary>
        /// A <see cref="DispatcherQueue"/> whose foreground thread to switch execution to.
        /// </summary>
        private readonly DispatcherQueue dispatcher;

        /// <summary>
        /// Specifies the priority for event dispatch.
        /// </summary>
        private readonly DispatcherQueuePriority priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="DispatcherQueueThreadSwitcher"/> struct.
        /// </summary>
        /// <param name="dispatcher">A <see cref="DispatcherQueue"/> whose foreground thread to switch execution to.</param>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        public DispatcherQueueThreadSwitcher(DispatcherQueue dispatcher, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal)
        {
            this.dispatcher = dispatcher;
            this.priority = priority;
        }

        /// <inheritdoc/>
        public bool IsCompleted => !(this.dispatcher is DispatcherQueue dispatcher)
            || (ThreadSwitcher.IsHasThreadAccessPropertyAvailable && dispatcher.HasThreadAccess);

        /// <inheritdoc/>
        public void GetResult() { }

        /// <inheritdoc/>
        public DispatcherQueueThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = dispatcher.TryEnqueue(priority, continuation.Invoke);
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="SynchronizationContext"/>. This type is not intended to be used directly from your code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct SynchronizationContextThreadSwitcher : IThreadSwitcher<SynchronizationContextThreadSwitcher>
    {
        /// <summary>
        /// Specifies the priority for event dispatch.
        /// </summary>
        private readonly SynchronizationContext context;

        /// <summary>
        /// Initializes a new instance of the <see cref="SynchronizationContextThreadSwitcher"/> struct.
        /// </summary>
        /// <param name="context">A <see cref="SynchronizationContext"/> whose foreground thread to switch execution to.</param>
        public SynchronizationContextThreadSwitcher(SynchronizationContext context) => this.context = context;

        /// <inheritdoc/>
        public bool IsCompleted => !(this.context is SynchronizationContext context)
            || SynchronizationContext.Current == context;

        /// <inheritdoc/>
        public void GetResult() { }

        /// <inheritdoc/>
        public SynchronizationContextThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => context.Post(_ => continuation(), null);
    }

    /// <summary>
    /// A helper type for switch thread by <see cref="ThreadPool"/>. This type is not intended to be used directly from your code.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public readonly struct ThreadPoolThreadSwitcher : IThreadSwitcher<ThreadPoolThreadSwitcher>
    {
        /// <summary>
        /// Specifies the priority for event dispatch.
        /// </summary>
        private readonly WorkItemPriority priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="ThreadPoolThreadSwitcher"/> struct.
        /// </summary>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        public ThreadPoolThreadSwitcher(WorkItemPriority priority = WorkItemPriority.Normal) => this.priority = priority;

        /// <inheritdoc/>
        public bool IsCompleted => SynchronizationContext.Current == null;

        /// <inheritdoc/>
        public void GetResult() { }

        /// <inheritdoc/>
        public ThreadPoolThreadSwitcher GetAwaiter() => this;

        /// <inheritdoc/>
        IThreadSwitcher IThreadSwitcher.GetAwaiter() => this;

        /// <inheritdoc/>
        public void OnCompleted(Action continuation) => _ = ThreadPool.RunAsync(_ => continuation(), priority);
    }

    /// <summary>
    /// The extensions for switching threads.
    /// </summary>
    public static class ThreadSwitcher
    {
        /// <summary>
        /// Gets is <see cref="DispatcherQueue.HasThreadAccess"/> supported.
        /// </summary>
        public static bool IsHasThreadAccessPropertyAvailable => ApiInfoHelper.IsHasThreadAccessSupported;

        /// <summary>
        /// A helper function—for use within a coroutine—that you can <see langword="await"/> to switch execution to a specific foreground thread. 
        /// </summary>
        /// <param name="dispatcher">A <see cref="DispatcherQueue"/> whose foreground thread to switch execution to.</param>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static DispatcherQueueThreadSwitcher ResumeForegroundAsync(this DispatcherQueue dispatcher, DispatcherQueuePriority priority = DispatcherQueuePriority.Normal) => new DispatcherQueueThreadSwitcher(dispatcher, priority);

        /// <summary>
        /// A helper function—for use within a coroutine—that you can <see langword="await"/> to switch execution to a specific foreground thread. 
        /// </summary>
        /// <param name="dispatcher">A <see cref="CoreDispatcher"/> whose foreground thread to switch execution to.</param>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static CoreDispatcherThreadSwitcher ResumeForegroundAsync(this CoreDispatcher dispatcher, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal) => new CoreDispatcherThreadSwitcher(dispatcher, priority);

        /// <summary>
        /// A helper function—for use within a coroutine—that you can <see langword="await"/> to switch execution to a specific foreground thread. 
        /// </summary>
        /// <param name="context">A <see cref="SynchronizationContext"/> whose foreground thread to switch execution to.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static SynchronizationContextThreadSwitcher ResumeForegroundAsync(this SynchronizationContext context) => new SynchronizationContextThreadSwitcher(context);

        /// <summary>
        /// A helper function—for use within a coroutine—that returns control to the caller, and then immediately resumes execution on a thread pool thread.
        /// </summary>
        /// <param name="priority">Specifies the priority for event dispatch.</param>
        /// <returns>An object that you can <see langword="await"/>.</returns>
        public static ThreadPoolThreadSwitcher ResumeBackgroundAsync(WorkItemPriority priority = WorkItemPriority.Normal) => new ThreadPoolThreadSwitcher(priority);
    }
}
using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoolapkLite.Common
{
    public class AsyncLock
    {
        private SemaphoreSlim m_semaphore;
        private readonly Task<Releaser> m_releaser;

        public AsyncLock(int initialCount = 1)
        {
            m_semaphore = new SemaphoreSlim(initialCount);
            m_releaser = Task.FromResult(new Releaser(this));
        }

        public Task<Releaser> LockAsync()
        {
            Task wait = m_semaphore.WaitAsync();
            return wait.IsCompleted ?
                m_releaser :
                wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public void SetSemaphoreSlim(int initialCount)
        {
            m_semaphore.Dispose();
            m_semaphore = new SemaphoreSlim(initialCount);
        }

        public readonly struct Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;

            internal Releaser(AsyncLock toRelease) => m_toRelease = toRelease;

            public void Dispose() => m_toRelease?.m_semaphore.Release();
        }
    }
}

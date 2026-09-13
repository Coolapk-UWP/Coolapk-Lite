using System;
using System.Threading;
using System.Threading.Tasks;

namespace CoolapkLite.Common
{
    public sealed class AsyncLock
    {
        private readonly SemaphoreSlim m_semaphore;

        public AsyncLock(int initialCount = 1) => m_semaphore = new SemaphoreSlim(initialCount);

        public async Task<Releaser> LockAsync(CancellationToken cancellationToken = default)
        {
            await m_semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new Releaser(this);
        }

        public readonly struct Releaser : IDisposable
        {
            private readonly AsyncLock m_toRelease;

            internal Releaser(AsyncLock toRelease) => m_toRelease = toRelease;

            public void Dispose() => m_toRelease?.m_semaphore.Release();
        }
    }
}

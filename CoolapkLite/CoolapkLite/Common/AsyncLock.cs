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
            return new Releaser(m_semaphore);
        }

        public readonly struct Releaser : IDisposable
        {
            private readonly SemaphoreSlim m_semaphore;

            internal Releaser(SemaphoreSlim semaphore) => m_semaphore = semaphore;

            public void Dispose() => m_semaphore.Release();
        }
    }
}

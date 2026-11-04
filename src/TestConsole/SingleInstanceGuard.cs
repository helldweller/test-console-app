using System;
using System.Threading;

namespace TestConsole;

/// <summary>
/// Holds a named mutex that marks this process as the only running instance.
/// </summary>
internal sealed class SingleInstanceGuard : IDisposable
{
    private readonly Mutex _mutex;

    private SingleInstanceGuard(Mutex mutex) => _mutex = mutex;

    /// <summary>
    /// Creates and takes ownership of the mutex <paramref name="mutexName"/>.
    /// </summary>
    /// <returns>The guard, or <see langword="null"/> if another process already holds the mutex.</returns>
    /// <exception cref="UnauthorizedAccessException">The mutex exists but was created by another user.</exception>
    public static SingleInstanceGuard? TryAcquire(string mutexName)
    {
        var mutex = new Mutex(initiallyOwned: true, mutexName, out bool createdNew);
        if (createdNew)
        {
            return new SingleInstanceGuard(mutex);
        }

        mutex.Dispose();
        return null;
    }

    public void Dispose()
    {
        _mutex.ReleaseMutex();
        _mutex.Dispose();
    }
}

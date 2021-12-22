using System;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Disposables
{
    /// <summary>
    ///     Marks a resource as disposable and provides a default implementation.
    /// </summary>
    public interface IDisposableResource : IDisposable, IAsyncDisposable
    {
        /// <inheritdoc/>
        void IDisposable.Dispose()
        {
            DisposeAsync().AsTask().Wait();
        }

        /// <inheritdoc/>
        ValueTask IAsyncDisposable.DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}

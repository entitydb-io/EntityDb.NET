using EntityDb.Abstractions.Disposables;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Common.Disposables;

internal record DisposableResourceBaseRecord : IDisposableResource
{
    [ExcludeFromCodeCoverage(Justification = "All Tests Use DisposeAsync")]
    public virtual void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

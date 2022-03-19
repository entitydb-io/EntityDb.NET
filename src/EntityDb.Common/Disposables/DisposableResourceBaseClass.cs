using EntityDb.Abstractions.Disposables;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace EntityDb.Common.Disposables;

internal class DisposableResourceBaseClass : IDisposableResource
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

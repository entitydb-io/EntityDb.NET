using EntityDb.Abstractions.Disposables;
using System.Threading.Tasks;

namespace EntityDb.Common.Disposables;

internal record DisposableResourceBaseRecord : IDisposableResource
{
    public virtual void Dispose()
    {
        DisposeAsync().AsTask().Wait();
    }

    public virtual ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

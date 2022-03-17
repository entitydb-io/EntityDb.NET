using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.InMemory.Sessions;

internal class ReadOnlyInMemorySession<TSnapshot> : IInMemorySession<TSnapshot>
{
    private readonly IInMemorySession<TSnapshot> _inMemorySession;

    public ReadOnlyInMemorySession(IInMemorySession<TSnapshot> inMemorySession)
    {
        _inMemorySession = inMemorySession;
    }
    
    public Task<bool> Insert(Id snapshotId, TSnapshot snapshot)
    {
        return Task.FromException<bool>(new CannotWriteInReadOnlyModeException());
    }

    public Task<TSnapshot?> Get(Id snapshotId)
    {
        return _inMemorySession.Get(snapshotId);
    }

    public Task<bool> Delete(IEnumerable<Id> snapshotIds)
    {
        return Task.FromException<bool>(new CannotWriteInReadOnlyModeException());
    }
}

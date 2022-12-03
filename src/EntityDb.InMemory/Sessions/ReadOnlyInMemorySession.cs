using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Exceptions;

namespace EntityDb.InMemory.Sessions;

internal class ReadOnlyInMemorySession<TSnapshot> : IInMemorySession<TSnapshot>
{
    private readonly IInMemorySession<TSnapshot> _inMemorySession;

    public ReadOnlyInMemorySession(IInMemorySession<TSnapshot> inMemorySession)
    {
        _inMemorySession = inMemorySession;
    }

    public Task<bool> Insert(Pointer snapshotPointer, TSnapshot snapshot)
    {
        return Task.FromException<bool>(new CannotWriteInReadOnlyModeException());
    }

    public Task<TSnapshot?> Get(Pointer snapshotPointer)
    {
        return _inMemorySession.Get(snapshotPointer);
    }

    public Task<bool> Delete(IEnumerable<Pointer> snapshotPointers)
    {
        return Task.FromException<bool>(new CannotWriteInReadOnlyModeException());
    }
}

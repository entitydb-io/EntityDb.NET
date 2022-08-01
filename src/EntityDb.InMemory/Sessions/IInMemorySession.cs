using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.InMemory.Sessions;

internal interface IInMemorySession<TSnapshot>
{
    Task<bool> Insert(Pointer snapshotPointer, TSnapshot snapshot);

    Task<TSnapshot?> Get(Pointer snapshotPointer);

    Task<bool> Delete(IEnumerable<Pointer> snapshotPointers);
}

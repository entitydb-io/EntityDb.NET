using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.EntityFramework.Sessions;

internal interface IEntityFrameworkSession<TSnapshot>
{
    Task Insert(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken);

    Task<TSnapshot?> Get(Pointer snapshotPointer, CancellationToken cancellationToken);

    Task Delete(IEnumerable<Pointer> snapshotPointers, CancellationToken cancellationToken);
}

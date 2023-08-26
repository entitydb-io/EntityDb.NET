using EntityDb.Abstractions.Disposables;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace EntityDb.EntityFramework.Sessions;

internal interface IEntityFrameworkSession<TSnapshot> : IDisposableResource
{
    internal DbContext DbContext { get; }

    IEntityFrameworkSession<TSnapshot> WithSnapshotSessionOptions(EntityFrameworkSnapshotSessionOptions snapshotSessionOptions);

    Task StartTransaction(CancellationToken cancellationToken);
    Task CommitTransaction(CancellationToken cancellationToken);
    Task AbortTransaction(CancellationToken cancellationToken);

    Task Upsert(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken);

    Task<TSnapshot?> Get(Pointer snapshotPointer, CancellationToken cancellationToken);

    Task Delete(IEnumerable<Pointer> snapshotPointers, CancellationToken cancellationToken);
}

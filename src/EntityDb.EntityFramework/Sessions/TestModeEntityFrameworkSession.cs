using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.EntityFramework.Sessions;

internal record TestModeEntityFrameworkSession<TSnapshot>(IEntityFrameworkSession<TSnapshot> EntityFrameworkSession) : DisposableResourceBaseRecord, IEntityFrameworkSession<TSnapshot>
{
    public Task StartTransaction(CancellationToken cancellationToken)
    {
        // Test Mode Transactions are started in the Test Mode Repository Factory
        return Task.CompletedTask;
    }

    public Task CommitTransaction(CancellationToken cancellationToken)
    {
        // Test Mode Transactions are never committed
        return Task.CompletedTask;
    }

    public Task AbortTransaction(CancellationToken cancellationToken)
    {
        // Test Mode Transactions are aborted in the Test Mode Repository Factory
        return Task.CompletedTask;
    }

    public Task Upsert(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken)
    {
        return EntityFrameworkSession.Upsert(snapshotPointer, snapshot, cancellationToken);
    }

    public Task<TSnapshot?> Get(Pointer snapshotPointer, CancellationToken cancellationToken)
    {
        return EntityFrameworkSession.Get(snapshotPointer, cancellationToken);
    }

    public Task Delete(IEnumerable<Pointer> snapshotPointers, CancellationToken cancellationToken)
    {
        return EntityFrameworkSession.Delete(snapshotPointers, cancellationToken);
    }

    public IEntityFrameworkSession<TSnapshot> WithSnapshotSessionOptions(EntityFrameworkSnapshotSessionOptions snapshotSessionOptions)
    {
        return this with { EntityFrameworkSession = EntityFrameworkSession.WithSnapshotSessionOptions(snapshotSessionOptions) };
    }
}

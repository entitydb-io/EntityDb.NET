using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.EntityFramework.Predicates;
using EntityDb.EntityFramework.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using Pointer = EntityDb.Abstractions.ValueObjects.Pointer;

namespace EntityDb.EntityFramework.Sessions;

internal class EntityFrameworkSession<TSnapshot, TDbContext> : DisposableResourceBaseClass, IEntityFrameworkSession<TSnapshot>
    where TSnapshot : class
    where TDbContext : SnapshotReferenceDbContext
{
    private readonly TDbContext _dbContext;
    private readonly EntityFrameworkSnapshotSessionOptions _options;

    private IDbContextTransaction? Transaction { get; set; }

    public EntityFrameworkSession(TDbContext dbContext, EntityFrameworkSnapshotSessionOptions options)
    {
        _dbContext = dbContext;
        _options = options;
    }

    private static Expression<Func<SnapshotReference<TSnapshot>, bool>> SnapshotPointerPredicate(Pointer snapshotPointer)
    {
        return snapshotReference =>
            snapshotReference.PointerId == snapshotPointer.Id &&
            snapshotReference.PointerVersionNumber == snapshotPointer.VersionNumber;
    }

    public async Task Delete(IEnumerable<Pointer> snapshotPointers, CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        await _dbContext.Set<SnapshotReference<TSnapshot>>()
            .Where(PredicateExpressionBuilder.Or(snapshotPointers, SnapshotPointerPredicate))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<TSnapshot?> Get(Pointer snapshotPointer, CancellationToken cancellationToken)
    {
        var reference = await _dbContext.Set<SnapshotReference<TSnapshot>>()
            .Where(SnapshotPointerPredicate(snapshotPointer))
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        return reference?.Snapshot;
    }

    public async Task Insert(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        var reference = new SnapshotReference<TSnapshot>
        {
            Id = Guid.NewGuid(),
            PointerId = snapshotPointer.Id,
            PointerVersionNumber = snapshotPointer.VersionNumber,
            Snapshot = snapshot
        };

        _dbContext.Set<SnapshotReference<TSnapshot>>().Add(reference);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private void AssertNotReadOnly()
    {
        if (_options.ReadOnly)
        {
            throw new CannotWriteInReadOnlyModeException();
        }
    }

    public static IEntityFrameworkSession<TSnapshot> Create
    (
        IServiceProvider serviceProvider,
        TDbContext dbContext,
        EntityFrameworkSnapshotSessionOptions options
    )
    {
        return ActivatorUtilities.CreateInstance<EntityFrameworkSession<TSnapshot, TDbContext>>(serviceProvider, dbContext, options);
    }

    public async Task StartTransaction(CancellationToken cancellationToken)
    {
        Transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransaction(CancellationToken cancellationToken)
    {
        if (Transaction != null)
        {
            await Transaction.CommitAsync(cancellationToken);
            await Transaction.DisposeAsync();

            Transaction = null;
        }
    }

    public async Task AbortTransaction(CancellationToken cancellationToken)
    {
        if (Transaction != null)
        {
            await Transaction.RollbackAsync(cancellationToken);
            await Transaction.DisposeAsync();

            Transaction = null;
        }
    }

    public IEntityFrameworkSession<TSnapshot> WithSnapshotSessionOptions(EntityFrameworkSnapshotSessionOptions snapshotSessionOptions)
    {
        return new EntityFrameworkSession<TSnapshot, TDbContext>(_dbContext, snapshotSessionOptions);
    }
}

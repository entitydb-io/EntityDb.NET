using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Snapshots;
using EntityDb.EntityFramework.Predicates;
using EntityDb.EntityFramework.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Pointer = EntityDb.Abstractions.ValueObjects.Pointer;

namespace EntityDb.EntityFramework.Sessions;

public interface IEntityFrameworkSnapshot<TSnapshot> : ISnapshot<TSnapshot>
{
    Expression<Func<TSnapshot, bool>> GetKeyPredicate();
}

internal class EntityFrameworkSession<TSnapshot, TDbContext> : DisposableResourceBaseClass, IEntityFrameworkSession<TSnapshot>
    where TSnapshot : class, IEntityFrameworkSnapshot<TSnapshot>
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

        var snapshotReferenceSet = _dbContext.Set<SnapshotReference<TSnapshot>>();
        var snapshotSet = _dbContext.Set<TSnapshot>();

        var snapshotReferences = await snapshotReferenceSet
            .Include(snapshotReference => snapshotReference.Snapshot)
            .Where(PredicateExpressionBuilder.Or(snapshotPointers, SnapshotPointerPredicate))
            .ToArrayAsync(cancellationToken);

        foreach (var snapshotReference in snapshotReferences)
        {
            snapshotReferenceSet.Remove(snapshotReference);

            var anyRelatedSnapshotReferences = await snapshotReferenceSet
                .Where
                (
                    relatedSnapshotReference =>
                        relatedSnapshotReference.Id != snapshotReference.Id &&
                        relatedSnapshotReference.SnapshotId == snapshotReference.SnapshotId &&
                        relatedSnapshotReference.SnapshotVersionNumber == snapshotReference.SnapshotVersionNumber
                )
                .AnyAsync(cancellationToken);

            if (!anyRelatedSnapshotReferences)
            {
                snapshotSet.Remove(snapshotReference.Snapshot);
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TSnapshot?> Get(Pointer snapshotPointer, CancellationToken cancellationToken)
    {
        var snapshotReference = await _dbContext.Set<SnapshotReference<TSnapshot>>()
            .Include(reference => reference.Snapshot)
            .AsNoTracking()
            .Where(SnapshotPointerPredicate(snapshotPointer))
            .SingleOrDefaultAsync(cancellationToken);

        return snapshotReference?.Snapshot;
    }

    public async Task Upsert(Pointer snapshotPointer, TSnapshot snapshot, CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        var snapshotSet = _dbContext.Set<TSnapshot>();

        var sapshotExists = await snapshotSet
            .Where(snapshot.GetKeyPredicate())
            .AnyAsync(cancellationToken);

        if (!sapshotExists)
        {
            snapshotSet.Add(snapshot);
        }

        var snapshotReferenceSet = _dbContext.Set<SnapshotReference<TSnapshot>>();

        var previousSnapshotReference = await snapshotReferenceSet
            .Where(SnapshotPointerPredicate(snapshotPointer))
            .SingleOrDefaultAsync(cancellationToken);

        if (previousSnapshotReference != null)
        {
            previousSnapshotReference.SnapshotId = snapshot.GetId();
            previousSnapshotReference.SnapshotVersionNumber = snapshot.GetVersionNumber();
            previousSnapshotReference.Snapshot = snapshot;
        }
        else
        {
            var snapshotReference = new SnapshotReference<TSnapshot>
            {
                Id = Guid.NewGuid(),
                PointerId = snapshotPointer.Id,
                PointerVersionNumber = snapshotPointer.VersionNumber,
                SnapshotId = snapshot.GetId(),
                SnapshotVersionNumber = snapshot.GetVersionNumber(),
                Snapshot = snapshot,
            };

            snapshotReferenceSet.Add(snapshotReference);
        }

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

    [ExcludeFromCodeCoverage(Justification =
        "Tests should run with the Debug configuration, and should not execute this method.")]
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

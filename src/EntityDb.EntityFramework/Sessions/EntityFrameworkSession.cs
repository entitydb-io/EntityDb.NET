using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.EntityFramework.Predicates;
using EntityDb.EntityFramework.Snapshots;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;
using Pointer = EntityDb.Abstractions.ValueObjects.Pointer;

namespace EntityDb.EntityFramework.Sessions;

internal class EntityFrameworkSession<TSnapshot, TDbContext> : DisposableResourceBaseClass, IEntityFrameworkSession<TSnapshot>
    where TSnapshot : class
    where TDbContext : DbContext, ISnapshotDbContext<TSnapshot>
{
    private readonly TDbContext _dbContext;
    private readonly EntityFrameworkSnapshotSessionOptions _options;

    public EntityFrameworkSession(TDbContext dbContext, EntityFrameworkSnapshotSessionOptions options)
    {
        _dbContext = dbContext;
        _options = options;
    }

    private static Expression<Func<SnapshotReference<TSnapshot>, bool>> SnapshotPointerPredicate(Pointer snapshotPointer)
    {
        return snapshotReference =>
            snapshotReference.PointerId == snapshotPointer.Id.Value &&
            snapshotReference.PointerVersionNumber == snapshotPointer.VersionNumber.Value;
    }

    public async Task Delete(IEnumerable<Pointer> snapshotPointers, CancellationToken cancellationToken)
    {
        AssertNotReadOnly();

        await _dbContext.SnapshotReferences
            .Where(PredicateExpressionBuilder.Or(snapshotPointers, SnapshotPointerPredicate))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<TSnapshot?> Get(Pointer snapshotPointer, CancellationToken cancellationToken)
    {
        var reference = await _dbContext.SnapshotReferences
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
            PointerId = snapshotPointer.Id.Value,
            PointerVersionNumber = snapshotPointer.VersionNumber.Value,
            Snapshot = snapshot
        };

        _dbContext.SnapshotReferences.Add(reference);

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
}

using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Queries.Standard;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Entities;

internal class EntityRepository<TEntity> : DisposableResourceBaseClass, IEntityRepository<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IEnumerable<ISourceSubscriber> _sourceSubscribers;

    public EntityRepository
    (
        IEnumerable<ISourceSubscriber> sourceSubscribers,
        ISourceRepository sourceRepository,
        ISnapshotRepository<TEntity>? snapshotRepository = null
    )
    {
        _sourceSubscribers = sourceSubscribers;
        SourceRepository = sourceRepository;
        SnapshotRepository = snapshotRepository;
    }

    public ISourceRepository SourceRepository { get; }
    public ISnapshotRepository<TEntity>? SnapshotRepository { get; }

    public async Task<TEntity> GetSnapshot(Pointer entityPointer, CancellationToken cancellationToken = default)
    {
        var snapshot = SnapshotRepository is not null
            ? await SnapshotRepository.GetSnapshotOrDefault(entityPointer, cancellationToken) ??
              TEntity.Construct(entityPointer.Id)
            : TEntity.Construct(entityPointer.Id);

        var snapshotPointer = snapshot.GetPointer();

        var query = new GetDeltasQuery(entityPointer, snapshotPointer.Version);

        var deltas = SourceRepository.EnumerateDeltas(query, cancellationToken);

        var entity = await deltas
            .AggregateAsync
            (
                snapshot,
                (current, delta) => current.Reduce(delta),
                cancellationToken
            );

        if (!entityPointer.IsSatisfiedBy(entity.GetPointer()))
        {
            throw new SnapshotPointerDoesNotExistException();
        }

        return entity;
    }

    public async Task<bool> Commit(Source source,
        CancellationToken cancellationToken = default)
    {
        var success = await SourceRepository.Commit(source, cancellationToken);

        if (success)
        {
            Publish(source);
        }

        return success;
    }

    public override async ValueTask DisposeAsync()
    {
        await SourceRepository.DisposeAsync();

        if (SnapshotRepository is not null)
        {
            await SnapshotRepository.DisposeAsync();
        }
    }

    private void Publish(Source source)
    {
        foreach (var sourceSubscriber in _sourceSubscribers)
        {
            sourceSubscriber.Notify(source);
        }
    }

    public static EntityRepository<TEntity> Create
    (
        IServiceProvider serviceProvider,
        ISourceRepository sourceRepository,
        ISnapshotRepository<TEntity>? snapshotRepository = null
    )
    {
        if (snapshotRepository is null)
        {
            return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(serviceProvider,
                sourceRepository);
        }

        return ActivatorUtilities.CreateInstance<EntityRepository<TEntity>>(serviceProvider, sourceRepository,
            snapshotRepository);
    }
}

using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Entities.Deltas;
using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Queries.Standard;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Immutable;

namespace EntityDb.Common.Entities;

internal class MultipleEntityRepository<TEntity> : DisposableResourceBaseClass, IMultipleEntityRepository<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IAgent _agent;
    private readonly Dictionary<Id, TEntity> _knownEntities = new();
    private readonly List<Message> _messages = new();

    public MultipleEntityRepository
    (
        IAgent agent,
        ISourceRepository sourceRepository,
        ISnapshotRepository<TEntity>? snapshotRepository = null
    )
    {
        _agent = agent;
        SourceRepository = sourceRepository;
        SnapshotRepository = snapshotRepository;
    }

    public ISourceRepository SourceRepository { get; }
    public ISnapshotRepository<TEntity>? SnapshotRepository { get; }

    public void Create(Id entityId)
    {
        if (_knownEntities.ContainsKey(entityId))
        {
            throw new EntityAlreadyLoadedException();
        }

        var entity = TEntity.Construct(entityId);

        _knownEntities.Add(entityId, entity);
    }
    
    public async Task Load(Pointer entityPointer, CancellationToken cancellationToken = default)
    {
        if (_knownEntities.ContainsKey(entityPointer.Id))
        {
            throw new EntityAlreadyLoadedException();
        }
        
        var snapshot = SnapshotRepository is not null
            ? await SnapshotRepository.GetSnapshotOrDefault(entityPointer, cancellationToken) ??
              TEntity.Construct(entityPointer.Id)
            : TEntity.Construct(entityPointer.Id);

        var snapshotPointer = snapshot.GetPointer();

        var query = new GetDeltasQuery(entityPointer, snapshotPointer.Version);

        var entity = await SourceRepository
            .EnumerateDeltas(query, cancellationToken)
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

        _knownEntities.Add(entityPointer.Id, entity);
    }

    public TEntity Get(Id entityId)
    {
        if (!_knownEntities.TryGetValue(entityId, out var entity))
        {
            throw new EntityNotLoadedException();
        }
        
        return entity;
    }
    
    public void Append(Id entityId, object delta)
    {
        if (!_knownEntities.TryGetValue(entityId, out var entity))
        {
            throw new EntityNotLoadedException();
        }

        entity = entity.Reduce(delta);

        _messages.Add(new Message
        {
            Id = Id.NewId(),
            EntityPointer = entity.GetPointer(),
            Delta = delta,
            AddLeases = delta is IAddLeasesDelta<TEntity> addLeasesDelta
                ? addLeasesDelta.GetLeases(entity).ToImmutableArray()
                : ImmutableArray<ILease>.Empty,
            AddTags = delta is IAddTagsDelta<TEntity> addTagsDelta
                ? addTagsDelta.GetTags(entity).ToImmutableArray()
                : ImmutableArray<ITag>.Empty,
            DeleteLeases = delta is IDeleteLeasesDelta<TEntity> deleteLeasesDelta
                ? deleteLeasesDelta.GetLeases(entity).ToImmutableArray()
                : ImmutableArray<ILease>.Empty,
            DeleteTags = delta is IDeleteTagsDelta<TEntity> deleteTagsDelta
                ? deleteTagsDelta.GetTags(entity).ToImmutableArray()
                : ImmutableArray<ITag>.Empty,
        });

        _knownEntities[entityId] = entity;
    }
    
    public async Task<bool> Commit(Id sourceId,
        CancellationToken cancellationToken = default)
    {
        var source = new Source
        {
            Id = sourceId,
            TimeStamp = _agent.TimeStamp,
            AgentSignature = _agent.Signature,
            Messages = _messages.ToImmutableArray(),
        };

        _messages.Clear();
        
        return await SourceRepository.Commit(source, cancellationToken);
    }

    public override async ValueTask DisposeAsync()
    {
        await SourceRepository.DisposeAsync();

        if (SnapshotRepository is not null)
        {
            await SnapshotRepository.DisposeAsync();
        }
    }

    public static MultipleEntityRepository<TEntity> Create
    (
        IServiceProvider serviceProvider,
        IAgent agent,
        ISourceRepository sourceRepository,
        ISnapshotRepository<TEntity>? snapshotRepository = null
    )
    {
        if (snapshotRepository is null)
        {
            return ActivatorUtilities.CreateInstance<MultipleEntityRepository<TEntity>>
            (
                serviceProvider,
                agent,
                sourceRepository
            );
        }

        return ActivatorUtilities.CreateInstance<MultipleEntityRepository<TEntity>>
        (
            serviceProvider, 
            agent,
            sourceRepository,
            snapshotRepository
        );
    }
}

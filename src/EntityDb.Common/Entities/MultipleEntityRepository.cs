using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.States.Deltas;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Queries.Standard;
using Microsoft.Extensions.DependencyInjection;

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
        IStateRepository<TEntity>? stateRepository = null
    )
    {
        _agent = agent;
        SourceRepository = sourceRepository;
        StateRepository = stateRepository;
    }

    public ISourceRepository SourceRepository { get; }
    public IStateRepository<TEntity>? StateRepository { get; }

    public void Create(Id entityId)
    {
        if (_knownEntities.ContainsKey(entityId))
        {
            throw new ExistingEntityException();
        }

        var entity = TEntity.Construct(entityId);

        _knownEntities.Add(entityId, entity);
    }

    public async Task Load(Pointer entityPointer, CancellationToken cancellationToken = default)
    {
        if (_knownEntities.ContainsKey(entityPointer.Id))
        {
            throw new ExistingEntityException();
        }

        var state = StateRepository is not null
            ? await StateRepository.Get(entityPointer, cancellationToken) ??
              TEntity.Construct(entityPointer.Id)
            : TEntity.Construct(entityPointer.Id);

        var statePointer = state.GetPointer();

        var query = new GetDeltasDataQuery(entityPointer, statePointer.Version);

        var entity = await SourceRepository
            .EnumerateDeltas(query, cancellationToken)
            .AggregateAsync
            (
                state,
                (current, delta) => current.Reduce(delta),
                cancellationToken
            );

        if (!entityPointer.IsSatisfiedBy(entity.GetPointer()))
        {
            throw new StateDoesNotExistException();
        }

        _knownEntities.Add(entityPointer.Id, entity);
    }

    public TEntity Get(Id entityId)
    {
        if (!_knownEntities.TryGetValue(entityId, out var entity))
        {
            throw new UnknownEntityIdException();
        }

        return entity;
    }

    public void Append(Id entityId, object delta)
    {
        if (!_knownEntities.TryGetValue(entityId, out var entity))
        {
            throw new UnknownEntityIdException();
        }

        entity = entity.Reduce(delta);

        _messages.Add(new Message
        {
            Id = Id.NewId(),
            StatePointer = entity.GetPointer(),
            Delta = delta,
            AddLeases = delta is IAddLeasesDelta<TEntity> addLeasesDelta
                ? addLeasesDelta.GetLeases(entity).ToArray()
                : Array.Empty<ILease>(),
            AddTags = delta is IAddTagsDelta<TEntity> addTagsDelta
                ? addTagsDelta.GetTags(entity).ToArray()
                : Array.Empty<ITag>(),
            DeleteLeases = delta is IDeleteLeasesDelta<TEntity> deleteLeasesDelta
                ? deleteLeasesDelta.GetLeases(entity).ToArray()
                : Array.Empty<ILease>(),
            DeleteTags = delta is IDeleteTagsDelta<TEntity> deleteTagsDelta
                ? deleteTagsDelta.GetTags(entity).ToArray()
                : Array.Empty<ITag>(),
        });

        _knownEntities[entityId] = entity;
    }

    public async Task<bool> Commit(CancellationToken cancellationToken = default)
    {
        if (_messages.Count == 0)
        {
            return true;
        }
        
        var source = new Source
        {
            Id = Id.NewId(),
            TimeStamp = _agent.TimeStamp,
            AgentSignature = _agent.Signature,
            Messages = _messages.ToArray(),
        };

        var committed = await SourceRepository.Commit(source, cancellationToken);

        if (!committed)
        {
            return false;
        }

        _messages.Clear();

        return true;
    }

    public override async ValueTask DisposeAsync()
    {
        await SourceRepository.DisposeAsync();

        if (StateRepository is not null)
        {
            await StateRepository.DisposeAsync();
        }
    }

    public static MultipleEntityRepository<TEntity> Create
    (
        IServiceProvider serviceProvider,
        IAgent agent,
        ISourceRepository sourceRepository,
        IStateRepository<TEntity>? stateRepository = null
    )
    {
        if (stateRepository is null)
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
            stateRepository
        );
    }
}

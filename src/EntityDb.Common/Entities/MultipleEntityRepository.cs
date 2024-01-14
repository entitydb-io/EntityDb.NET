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

internal sealed class MultipleEntityRepository<TEntity> : DisposableResourceBaseClass, IMultipleEntityRepository<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly string _agentSignatureOptionsName;
    private readonly IAgentAccessor _agentAccessor;
    private readonly Dictionary<Id, TEntity> _knownEntities = new();
    private readonly List<Message> _messages = new();

    public MultipleEntityRepository
    (
        IAgentAccessor agentAccessor,
        string agentSignatureOptionsName,
        ISourceRepository sourceRepository,
        IStateRepository<TEntity>? stateRepository = null
    )
    {
        _agentSignatureOptionsName = agentSignatureOptionsName;
        _agentAccessor = agentAccessor;
        
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

        _messages.Add(Message.NewMessage(entity, entity.GetPointer(), delta));

        _knownEntities[entityId] = entity;
    }

    public async Task<bool> Commit(CancellationToken cancellationToken = default)
    {
        if (_messages.Count == 0)
        {
            return true;
        }

        var agent = await _agentAccessor.GetAgent(_agentSignatureOptionsName, cancellationToken);
        
        var source = new Source
        {
            Id = Id.NewId(),
            TimeStamp = agent.TimeStamp,
            AgentSignature = agent.Signature,
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
}

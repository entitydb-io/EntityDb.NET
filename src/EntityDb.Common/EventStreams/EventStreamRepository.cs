using EntityDb.Abstractions.EventStreams;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Sources.Attributes;
using EntityDb.Common.Sources.Queries.Standard;
using System.Collections.Immutable;

namespace EntityDb.Common.EventStreams;

internal sealed class EventStreamRepository : DisposableResourceBaseClass, IEventStreamRepository
{
    private const string RootScope = "EventStream";
    private const string StreamIdLabel = "StreamId";
    private const string EventIdLabel = "EventId";
    private readonly IAgent _agent;
    private readonly Dictionary<Key, Id> _knownEntityIds = new();
    private readonly List<Message> _messages = new();

    public EventStreamRepository(IAgent agent, ISourceRepository sourceRepository)
    {
        _agent = agent;

        SourceRepository = sourceRepository;
    }

    public ISourceRepository SourceRepository { get; }

    public async Task<bool> Stage(Key streamKey, Key eventKey, object delta,
        CancellationToken cancellationToken = default)
    {
        var eventKeyLease = GetEventKeyLease(streamKey, eventKey);

        var entityPointer = await GetEntityPointer(eventKeyLease, cancellationToken);

        if (entityPointer != default)
        {
            return false;
        }

        var addLeases = new List<ILease> { eventKeyLease };

        var (found, entityId) = await GetEntityId(streamKey, cancellationToken);

        if (!found)
        {
            addLeases.Add(GetStreamKeyLease(streamKey));
        }

        _messages.Add(new Message
        {
            Id = Id.NewId(), EntityPointer = entityId, Delta = delta, AddLeases = addLeases.ToImmutableArray(),
        });

        return true;
    }

    public async Task<bool> Commit(Id sourceId, byte maxAttempts = 3, CancellationToken cancellationToken = default)
    {
        var source = new Source
        {
            Id = sourceId,
            TimeStamp = _agent.TimeStamp,
            AgentSignature = _agent.Signature,
            Messages = _messages.ToImmutableArray(),
        };

        _messages.Clear();

        byte attempt = 1;

        while (true)
        {
            var committed = await SourceRepository.Commit(source, cancellationToken);

            if (committed)
            {
                return committed;
            }

            attempt += 1;

            if (attempt == maxAttempts)
            {
                break;
            }

            await Task.Delay(100 * attempt, cancellationToken);
        }

        return false;
    }

    public override ValueTask DisposeAsync()
    {
        return SourceRepository.DisposeAsync();
    }

    public static ILease GetStreamKeyLease(Key streamKey)
    {
        return new Lease(RootScope, StreamIdLabel, streamKey.Value);
    }

    public static ILease GetEventKeyLease(Key streamKey, Key eventKey)
    {
        return new Lease($"{RootScope}/{streamKey}", EventIdLabel, eventKey.Value);
    }

    private async Task<Pointer> GetEntityPointer(ILease lease, CancellationToken cancellationToken)
    {
        var query = new MatchingLeaseQuery(lease);

        return await SourceRepository
            .EnumerateEntityPointers(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<(bool, Id)> GetEntityId(Key streamKey, CancellationToken cancellationToken)
    {
        if (_knownEntityIds.TryGetValue(streamKey, out var entityId))
        {
            return (true, entityId);
        }

        var streamKeyLease = GetStreamKeyLease(streamKey);

        var entityPointer = await GetEntityPointer(streamKeyLease, cancellationToken);

        var found = entityPointer != default;

        entityId = found ? entityPointer.Id : Id.NewId();

        _knownEntityIds.Add(streamKey, entityId);

        return (found, entityId);
    }
}

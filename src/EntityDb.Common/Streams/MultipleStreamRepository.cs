using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.States.Attributes;
using EntityDb.Abstractions.Streams;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Queries.Standard;
using EntityDb.Common.States.Attributes;

namespace EntityDb.Common.Streams;

internal sealed class MultipleStreamRepository : DisposableResourceBaseClass, IMultipleStreamRepository
{
    private const string RootScope = "Stream";
    private const string StreamKeyLabel = "StreamKey";
    private const string MessageKeyLabel = "MessageKey";
    private readonly IAgent _agent;
    private readonly Dictionary<Key, Stream> _knownStreams = new();
    private readonly List<Message> _messages = new();

    public MultipleStreamRepository(IAgent agent, ISourceRepository sourceRepository)
    {
        _agent = agent;

        SourceRepository = sourceRepository;
    }

    public ISourceRepository SourceRepository { get; }

    public async Task LoadOrCreate(Key streamKey, CancellationToken cancellationToken = default)
    {
        if (_knownStreams.ContainsKey(streamKey))
        {
            throw new ExistingStreamException();
        }

        var streamKeyLease = GetStreamKeyLease(streamKey);

        var streamPointer = await GetStreamPointer(streamKeyLease, cancellationToken);

        var stream = streamPointer == default
            ? new Stream { Key = streamKey, Id = Id.NewId(), New = true }
            : new Stream { Key = streamKey, Id = streamPointer.Id, New = false };

        _knownStreams.Add(streamKey, stream);
    }

    public async Task<bool> Stage(Key streamKey, Key messageKey, object delta,
        CancellationToken cancellationToken = default)
    {
        if (!_knownStreams.TryGetValue(streamKey, out var stream))
        {
            throw new UnknownStreamException();
        }

        var messageKeyLease = GetMessageKeyLease(streamKey, messageKey);

        var statePointer = await GetStreamPointer(messageKeyLease, cancellationToken);

        if (statePointer != default)
        {
            return false;
        }

        var addLeases = new List<ILease> { messageKeyLease };

        if (stream.New)
        {
            addLeases.Add(GetStreamKeyLease(stream.Key));

            _knownStreams[streamKey] = stream with { New = false };
        }

        _messages.Add(new Message
        {
            Id = Id.NewId(), StatePointer = stream.Id, Delta = delta, AddLeases = addLeases.ToArray(),
        });

        return true;
    }

    public async Task<bool> Commit(CancellationToken cancellationToken = default)
    {
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

    public override ValueTask DisposeAsync()
    {
        return SourceRepository.DisposeAsync();
    }

    public static ILease GetStreamKeyLease(Key streamKey)
    {
        return new Lease(RootScope, StreamKeyLabel, streamKey.Value);
    }

    public static ILease GetMessageKeyLease(Key streamKey, Key messageKey)
    {
        return new Lease($"{RootScope}/{streamKey}", MessageKeyLabel, messageKey.Value);
    }

    private async Task<Pointer> GetStreamPointer(ILease lease, CancellationToken cancellationToken)
    {
        var query = new MatchingLeaseQuery(lease);

        return await SourceRepository
            .EnumerateStatePointers(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);
    }
}

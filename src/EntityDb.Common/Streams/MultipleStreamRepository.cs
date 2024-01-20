using EntityDb.Abstractions;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.States.Deltas;
using EntityDb.Abstractions.Streams;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Sources.Queries.Standard;

namespace EntityDb.Common.Streams;

internal sealed class MultipleStreamRepository : DisposableResourceBaseClass, IMultipleStreamRepository
{
    private readonly IAgentAccessor _agentAccessor;
    private readonly string _agentSignatureOptionsName;
    private readonly Dictionary<IStateKey, Stream> _knownStreams = new();
    private readonly List<Message> _messages = new();

    public MultipleStreamRepository(IAgentAccessor agentAccessor, string agentSignatureOptionsName,
        ISourceRepository sourceRepository)
    {
        _agentSignatureOptionsName = agentSignatureOptionsName;
        _agentAccessor = agentAccessor;

        SourceRepository = sourceRepository;
    }

    public ISourceRepository SourceRepository { get; }

    public async Task LoadOrCreate(IStateKey streamKey, CancellationToken cancellationToken = default)
    {
        if (_knownStreams.ContainsKey(streamKey))
        {
            throw new ExistingStreamException();
        }

        var streamKeyLease = streamKey.ToLease();

        var streamPointer = await GetStreamPointer(streamKeyLease, cancellationToken);

        var isNew = streamPointer == default;

        _knownStreams.Add(streamKey,
            new Stream { Key = streamKey, Id = isNew ? Id.NewId() : streamPointer.Id, IsNew = isNew });
    }

    public async Task Load(IStateKey streamKey, CancellationToken cancellationToken = default)
    {
        if (_knownStreams.ContainsKey(streamKey))
        {
            throw new ExistingStreamException();
        }

        var streamKeyLease = streamKey.ToLease();

        var streamPointer = await GetStreamPointer(streamKeyLease, cancellationToken);

        if (streamPointer == default)
        {
            throw new UnknownStreamKeyException();
        }

        _knownStreams.Add(streamKey, new Stream { Key = streamKey, Id = streamPointer.Id, IsNew = false });
    }

    public void Create(IStateKey streamKey)
    {
        if (_knownStreams.ContainsKey(streamKey))
        {
            throw new ExistingStreamException();
        }

        _knownStreams.Add(streamKey, new Stream { Key = streamKey, Id = Id.NewId(), IsNew = true });
    }

    public void Append(IStateKey streamKey, object delta)
    {
        if (!_knownStreams.TryGetValue(streamKey, out var stream))
        {
            throw new UnknownStreamKeyException();
        }

        var nextStreamPointer = stream.GetNextPointer();

        _messages.Add(Message.NewMessage<IStream>(stream, nextStreamPointer, delta, streamKey));
    }
    
    public async Task<bool> Append<TDelta>(IStateKey streamKey, TDelta delta, CancellationToken cancellationToken = default)
        where TDelta : IAddMessageKeyDelta
    {
        if (!_knownStreams.TryGetValue(streamKey, out var stream))
        {
            throw new UnknownStreamKeyException();
        }

        if (delta.GetMessageKey() is { } messageKey)
        {
            var messageKeyLease = messageKey.ToLease(streamKey);

            var streamPointer = await GetStreamPointer(messageKeyLease, cancellationToken);

            if (streamPointer != default)
            {
                return false;
            }   
        }

        var nextStreamPointer = stream.GetNextPointer();

        _messages.Add(Message.NewMessage<IStream>(stream, nextStreamPointer, delta, streamKey));

        return true;
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

    public override ValueTask DisposeAsync()
    {
        return SourceRepository.DisposeAsync();
    }

    private async Task<StatePointer> GetStreamPointer(ILease lease, CancellationToken cancellationToken)
    {
        var query = new MatchingLeaseDataQuery(lease);

        return await SourceRepository
            .EnumerateStatePointers(query, cancellationToken)
            .SingleOrDefaultAsync(cancellationToken);
    }
}

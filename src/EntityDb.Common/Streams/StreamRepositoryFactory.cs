﻿using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Abstractions.Streams;

namespace EntityDb.Common.Streams;

internal sealed class StreamRepositoryFactory : IStreamRepositoryFactory
{
    private readonly IAgentAccessor _agentAccessor;
    private readonly ISourceRepositoryFactory _sourceRepositoryFactory;

    public StreamRepositoryFactory
    (
        IAgentAccessor agentAccessor,
        ISourceRepositoryFactory sourceRepositoryFactory
    )
    {
        _agentAccessor = agentAccessor;
        _sourceRepositoryFactory = sourceRepositoryFactory;
    }

    public async Task<ISingleStreamRepository> CreateSingle
    (
        IStateKey streamKey,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken = default
    )
    {
        var multipleStreamRepository =
            await CreateMultiple(agentSignatureOptionsName, sourceSessionOptionsName, cancellationToken);

        await multipleStreamRepository.LoadOrCreate(streamKey, cancellationToken);

        return new SingleStreamRepository(multipleStreamRepository, streamKey);
    }

    public async Task<ISingleStreamRepository> CreateSingleForNew
    (
        IStateKey streamKey,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken = default
    )
    {
        var multipleStreamRepository =
            await CreateMultiple(agentSignatureOptionsName, sourceSessionOptionsName, cancellationToken);

        multipleStreamRepository.Create(streamKey);

        return new SingleStreamRepository(multipleStreamRepository, streamKey);
    }

    public async Task<ISingleStreamRepository> CreateSingleForExisting
    (
        IStateKey streamKey,
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken = default
    )
    {
        var multipleStreamRepository =
            await CreateMultiple(agentSignatureOptionsName, sourceSessionOptionsName, cancellationToken);

        await multipleStreamRepository.Load(streamKey, cancellationToken);

        return new SingleStreamRepository(multipleStreamRepository, streamKey);
    }

    public async Task<IMultipleStreamRepository> CreateMultiple
    (
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken
    )
    {
        var sourceRepository = await _sourceRepositoryFactory
            .Create(sourceSessionOptionsName, cancellationToken);

        return new MultipleStreamRepository
        (
            _agentAccessor,
            agentSignatureOptionsName,
            sourceRepository
        );
    }
}

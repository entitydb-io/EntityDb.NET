using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using EntityDb.Abstractions.Streams;
using EntityDb.Abstractions.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Streams;

internal sealed class StreamRepositoryFactory : IStreamRepositoryFactory
{
    private readonly IAgentAccessor _agentAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISourceRepositoryFactory _sourceRepositoryFactory;

    public StreamRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IAgentAccessor agentAccessor,
        ISourceRepositoryFactory sourceRepositoryFactory
    )
    {
        _serviceProvider = serviceProvider;
        _agentAccessor = agentAccessor;
        _sourceRepositoryFactory = sourceRepositoryFactory;
    }

    public async Task<ISingleStreamRepository> CreateSingle
    (
        Key streamKey,
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

    public async Task<IMultipleStreamRepository> CreateMultiple
    (
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken
    )
    {
        var agent = await _agentAccessor.GetAgent(agentSignatureOptionsName, cancellationToken);

        var sourceRepository = await _sourceRepositoryFactory
            .Create(sourceSessionOptionsName, cancellationToken);

        return ActivatorUtilities.CreateInstance<MultipleStreamRepository>(_serviceProvider, agent, sourceRepository);
    }
}

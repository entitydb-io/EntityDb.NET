using EntityDb.Abstractions.EventStreams;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.Sources.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.EventStreams;

internal sealed class EventStreamRepositoryFactory : IEventStreamRepositoryFactory
{
    private readonly IAgentAccessor _agentAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISourceRepositoryFactory _sourceRepositoryFactory;

    public EventStreamRepositoryFactory
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

    public async Task<IEventStreamRepository> CreateRepository
    (
        string agentSignatureOptionsName,
        string sourceSessionOptionsName,
        CancellationToken cancellationToken
    )
    {
        var agent = await _agentAccessor.GetAgent(agentSignatureOptionsName, cancellationToken);

        var sourceRepository = await _sourceRepositoryFactory
            .CreateRepository(sourceSessionOptionsName, cancellationToken);

        return ActivatorUtilities.CreateInstance<EventStreamRepository>(_serviceProvider, agent, sourceRepository);
    }
}

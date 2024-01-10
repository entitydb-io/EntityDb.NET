using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EntityDb.Common.Sources.Processors;

/// <summary>
///     A source processor that persists projection states
/// </summary>
/// <typeparam name="TProjection">The type of the projection</typeparam>
public sealed class ProjectionStateSourceProcessor<TProjection> : ISourceProcessor
    where TProjection : IProjection<TProjection>
{
    private readonly ILogger<ProjectionStateSourceProcessor<TProjection>> _logger;
    private readonly IProjectionRepositoryFactory<TProjection> _projectionRepositoryFactory;
    private readonly string _stateSessionOptionsName;

    /// <ignore />
    public ProjectionStateSourceProcessor
    (
        ILogger<ProjectionStateSourceProcessor<TProjection>> logger,
        IProjectionRepositoryFactory<TProjection> projectionRepositoryFactory,
        string stateSessionOptionsName
    )
    {
        _logger = logger;
        _projectionRepositoryFactory = projectionRepositoryFactory;
        _stateSessionOptionsName = stateSessionOptionsName;
    }

    /// <inheritdoc />
    public async Task Process(Source source, CancellationToken cancellationToken)
    {
        await using var projectionRepository = await _projectionRepositoryFactory
            .CreateRepository(_stateSessionOptionsName, cancellationToken);

        if (projectionRepository.StateRepository is null)
        {
            _logger.LogWarning("State repository not enabled, skipping source processing.");

            return;
        }

        foreach (var stateId in TProjection.EnumerateRelevantStateIds(source))
        {
            var previousProjection = await projectionRepository.StateRepository
                .Get(stateId, cancellationToken);

            var nextProjection = previousProjection == null
                ? TProjection.Construct(stateId)
                : previousProjection;

            nextProjection.Mutate(source);

            var nextProjectionPointer = nextProjection.GetPointer();

            if (nextProjection.ShouldRecordAsLatest(previousProjection))
            {
                await projectionRepository.StateRepository.Put(nextProjectionPointer.Id, nextProjection,
                    cancellationToken);
            }

            if (nextProjection.ShouldRecord())
            {
                await projectionRepository.StateRepository.Put(nextProjectionPointer, nextProjection,
                    cancellationToken);
            }

            cancellationToken.ThrowIfCancellationRequested();
        }
    }

    internal static ProjectionStateSourceProcessor<TProjection> Create(IServiceProvider serviceProvider,
        string stateSessionOptionsName)
    {
        return ActivatorUtilities.CreateInstance<ProjectionStateSourceProcessor<TProjection>>(serviceProvider,
            stateSessionOptionsName);
    }
}

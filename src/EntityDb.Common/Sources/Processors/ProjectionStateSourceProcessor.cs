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

        foreach (var projectionId in TProjection.EnumerateProjectionIds(source).Distinct())
        {
            var projection = await projectionRepository.TryLoad(projectionId, cancellationToken)
                ? projectionRepository.Get(projectionId)
                : TProjection.Construct(projectionId);
            
            projection.Mutate(source);

            if (projection.ShouldPersist())
            {
                await projectionRepository.StateRepository.Put(projection.GetPointer(), projection, cancellationToken);
            }

            if (projection.ShouldPersistAsLatest())
            {
                await projectionRepository.StateRepository.Put(projectionId, projection, cancellationToken);
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

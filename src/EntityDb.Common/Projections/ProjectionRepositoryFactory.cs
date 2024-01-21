using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.States;

namespace EntityDb.Common.Projections;

internal sealed class ProjectionRepositoryFactory<TProjection> : IProjectionRepositoryFactory<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStateRepositoryFactory<TProjection>? _stateRepositoryFactory;

    public ProjectionRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IStateRepositoryFactory<TProjection>? stateRepositoryFactory = null
    )
    {
        _serviceProvider = serviceProvider;
        _stateRepositoryFactory = stateRepositoryFactory;
    }

    public async Task<IProjectionRepository<TProjection>> CreateRepository
    (
        string? stateSessionOptionsName = null,
        CancellationToken cancellationToken = default
    )
    {
        if (_stateRepositoryFactory is null || stateSessionOptionsName is null)
        {
            return ProjectionRepository<TProjection>.Create(_serviceProvider);
        }

        var stateRepository =
            await _stateRepositoryFactory.Create(stateSessionOptionsName, cancellationToken);

        return ProjectionRepository<TProjection>.Create(_serviceProvider,
            stateRepository);
    }
}

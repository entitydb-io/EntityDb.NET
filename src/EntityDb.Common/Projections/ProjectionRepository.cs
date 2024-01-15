using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.States;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Projections;

internal sealed class ProjectionRepository<TProjection> : DisposableResourceBaseClass,
    IProjectionRepository<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IServiceProvider _serviceProvider;

    public ProjectionRepository
    (
        IServiceProvider serviceProvider,
        IStateRepository<TProjection>? stateRepository = null
    )
    {
        _serviceProvider = serviceProvider;
        StateRepository = stateRepository;
    }

    public IStateRepository<TProjection>? StateRepository { get; }

    public async Task<TProjection> Get(StatePointer projectionPointer, CancellationToken cancellationToken = default)
    {
        var projection = StateRepository is not null
            ? await StateRepository.Get(projectionPointer, cancellationToken) ??
              TProjection.Construct(projectionPointer.Id)
            : TProjection.Construct(projectionPointer.Id);

        var sources = projection.EnumerateSources(_serviceProvider, projectionPointer, cancellationToken);

        await foreach (var source in sources)
        {
            projection.Mutate(source);
        }

        StateDoesNotExistException.ThrowIfNotAcceptable(projectionPointer, projection.GetPointer());

        return projection;
    }

    public static IProjectionRepository<TProjection> Create(IServiceProvider serviceProvider,
        IStateRepository<TProjection>? stateRepository = null)
    {
        if (stateRepository is null)
        {
            return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider);
        }

        return ActivatorUtilities.CreateInstance<ProjectionRepository<TProjection>>(serviceProvider,
            stateRepository);
    }
}

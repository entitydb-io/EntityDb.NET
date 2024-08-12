using EntityDb.Abstractions;
using EntityDb.Abstractions.Projections;
using EntityDb.Abstractions.States;
using EntityDb.Common.Disposables;
using EntityDb.Common.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Common.Projections;

internal sealed class ProjectionRepository<TProjection> : DisposableResourceBaseClass,
    IProjectionRepository<TProjection>
    where TProjection : IProjection<TProjection>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Id, TProjection> _knownProjections = new();

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

    public async Task<bool> TryLoad(StatePointer projectionPointer, CancellationToken cancellationToken = default)
    {
        var projectionId = projectionPointer.Id;
        
        if (_knownProjections.TryGetValue(projectionId, out var projection))
        {
            var knownProjectionPointer = projection.GetPointer();

            if (projectionPointer.IsSatisfiedBy(knownProjectionPointer))
            {
                return true;
            }
            
            if (projectionPointer.StateVersion.Value < knownProjectionPointer.StateVersion.Value)
            {
                return false;
            }
        }
        else if (StateRepository is not null)
        {
            projection = await StateRepository.Get(projectionPointer, cancellationToken) ??
                              TProjection.Construct(projectionId);
        }
        else
        {
            projection = TProjection.Construct(projectionId);
        }

        var sources = projection.EnumerateSources(_serviceProvider, projectionPointer, cancellationToken);

        await foreach (var source in sources)
        {
            projection.Mutate(source);
        }

        if (!projectionPointer.IsSatisfiedBy(projection.GetPointer()))
        {
            return false;
        }

        _knownProjections.Add(projectionId, projection);

        return true;
    }

    public TProjection Get(Id projectionId)
    {
        if (!_knownProjections.TryGetValue(projectionId, out var projection))
        {
            throw new UnknownProjectionException();
        }

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

using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Sources;
using EntityDb.Abstractions.States;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;

namespace EntityDb.Common.Entities;

internal sealed class SingleEntityRepository<TEntity> : DisposableResourceBaseClass, ISingleEntityRepository<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IMultipleEntityRepository<TEntity> _multipleEntityRepository;

    public SingleEntityRepository(IMultipleEntityRepository<TEntity> multipleEntityRepository, Pointer entityPointer)
    {
        _multipleEntityRepository = multipleEntityRepository;

        EntityPointer = entityPointer;
    }

    public ISourceRepository SourceRepository => _multipleEntityRepository.SourceRepository;
    public IStateRepository<TEntity>? StateRepository => _multipleEntityRepository.StateRepository;
    public Pointer EntityPointer { get; }

    public TEntity Get()
    {
        return _multipleEntityRepository.Get(EntityPointer.Id);
    }

    public void Append(object delta)
    {
        _multipleEntityRepository.Append(EntityPointer.Id, delta);
    }

    public Task<bool> Commit(CancellationToken cancellationToken = default)
    {
        return _multipleEntityRepository.Commit(cancellationToken);
    }

    public override ValueTask DisposeAsync()
    {
        return _multipleEntityRepository.DisposeAsync();
    }
}

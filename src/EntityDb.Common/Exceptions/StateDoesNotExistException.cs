using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Projections;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when <see cref="IEntityRepositoryFactory{TEntity}.CreateSingleForExisting" />,
///     <see cref="IMultipleEntityRepository{TEntity}.Load" />, or <see cref="IProjectionRepository{TProjection}.Get" />
///     cannot find the requested state.
/// </summary>
public sealed class StateDoesNotExistException : Exception;

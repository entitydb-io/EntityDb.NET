using EntityDb.Abstractions.Entities;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when <see cref="IMultipleEntityRepository{TEntity}.Create" /> or
///     <see cref="IMultipleEntityRepository{TEntity}.TryLoad" /> is called for an entity that already
///     exists in the repository.
/// </summary>
public sealed class ExistingEntityException : Exception;

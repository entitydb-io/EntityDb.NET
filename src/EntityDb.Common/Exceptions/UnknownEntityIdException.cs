using EntityDb.Abstractions.Entities;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when <see cref="IMultipleEntityRepository{TEntity}.Get" /> or
///     <see cref="IMultipleEntityRepository{TEntity}.Append" /> is called for an entity that
///     is not loaded into the repository.
/// </summary>
public sealed class UnknownEntityIdException : Exception;

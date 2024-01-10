using EntityDb.Common.Entities;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an entity id to
///     <see cref="EntitySourceBuilder{TEntity}.Load" />
///     with an entity id that has already been loaded.
/// </summary>
public sealed class EntityAlreadyLoadedException : Exception
{
}

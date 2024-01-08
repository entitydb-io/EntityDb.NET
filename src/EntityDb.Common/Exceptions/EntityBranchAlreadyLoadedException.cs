using EntityDb.Common.Entities;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when an actor passes an entity branch to
///     <see cref="EntitySourceBuilder{TEntity}.Load" />
///     with an entity branch that has already been loaded.
/// </summary>
public sealed class EntityBranchAlreadyLoadedException : Exception
{
}

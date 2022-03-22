using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Entities;

/// <summary>
///     Provides basic functionality for the common implementations.
/// </summary>
/// <typeparam name="TEntity"></typeparam>
public interface IEntity<out TEntity>
{
    /// <summary>
    ///     Creates a new instance of a <typeparamref name="TEntity" />.
    /// </summary>
    /// <param name="entityId">The id of the entity.</param>
    /// <returns>A new instance of <typeparamref name="TEntity" />.</returns>
    abstract static TEntity Construct(Id entityId);

    /// <summary>
    ///     Returns the id of the entity.
    /// </summary>
    /// <returns>The id of this entity.</returns>
    Id GetId();
    
    /// <summary>
    ///     Returns the version number of the entity.
    /// </summary>
    /// <returns>The id of this entity.</returns>
    VersionNumber GetVersionNumber();
    
    /// <summary>
    ///     Returns a new <typeparamref name="TEntity" /> that incorporates the commands.
    /// </summary>
    /// <param name="commands">The commands</param>
    /// <returns>A new <typeparamref name="TEntity" /> that incorporates <paramref name="commands"/>.</returns>
    TEntity Reduce(params object[] commands);
}

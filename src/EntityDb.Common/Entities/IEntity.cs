using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Entities;

/// <summary>
///     Indicates the entity is compatible with several EntityDb.Common implementations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntity<TEntity> : ISnapshot<TEntity>
{
    /// <summary>
    ///     Returns a new <typeparamref name="TEntity" /> that incorporates the commands.
    /// </summary>
    /// <param name="commands">The commands</param>
    /// <returns>A new <typeparamref name="TEntity" /> that incorporates <paramref name="commands" />.</returns>
    TEntity Reduce(params object[] commands);
}

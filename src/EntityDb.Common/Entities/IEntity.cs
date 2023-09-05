using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Entities;

/// <summary>
///     Indicates the entity is compatible with several EntityDb.Common implementations.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
public interface IEntity<TEntity> : ISnapshot<TEntity>
{
    /// <ignore/>
    [Obsolete("Please use Reduce(object) instead. This method is no longer in supported.", true)]
    TEntity Reduce(params object[] command)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    ///     Returns a new <typeparamref name="TEntity" /> that incorporates the commands.
    /// </summary>
    /// <param name="command">The command</param>
    /// <returns>A new <typeparamref name="TEntity" /> that incorporates <paramref name="command" />.</returns>
    TEntity Reduce(object command);
}

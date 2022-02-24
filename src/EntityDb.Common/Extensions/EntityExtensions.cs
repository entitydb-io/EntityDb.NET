using EntityDb.Abstractions.Commands;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Extensions;

/// <summary>
///     Extension methods for entities.
/// </summary>
public static class EntityExtensions
{
    /// <summary>
    ///     Returns a new instance of <typeparamref name="TEntity" /> that incorporates the modifications of a command into an
    ///     entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be modified.</typeparam>
    /// <param name="entity">The entity to be modified.</param>
    /// <param name="command">The command to execute.</param>
    /// <returns>
    ///     A new instance of <typeparamref name="TEntity" /> that incorporates the modifications of
    ///     <paramref name="command" /> into <paramref name="entity" />.
    /// </returns>
    /// <remarks>
    ///     This method is ONLY intended to be used for business logic tests.
    /// </remarks>
    public static TEntity Reduce<TEntity>(this TEntity entity, ICommand<TEntity> command)
    {
        return command.Reduce(entity);
    }

    /// <summary>
    ///     Returns a new instance of <typeparamref name="TEntity" /> that incorporates the modifications of a set of commands
    ///     into an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be modified.</typeparam>
    /// <param name="entity">The entity to be modified.</param>
    /// <param name="commands">The set of commands to execute.</param>
    /// <returns>
    ///     A new instance of <typeparamref name="TEntity" /> that incorporates the modifications of
    ///     <paramref name="commands" /> into <paramref name="entity" />.
    /// </returns>
    /// <remarks>
    ///     This method is ONLY intended to be used for business logic tests.
    /// </remarks>
    public static TEntity Reduce<TEntity>(this TEntity entity, IEnumerable<ICommand<TEntity>> commands)
    {
        return commands.Aggregate(entity, Reduce);
    }
}

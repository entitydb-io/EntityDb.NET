using EntityDb.Abstractions.Facts;
using System.Collections.Generic;

namespace EntityDb.Abstractions.Commands
{
    /// <summary>
    ///     Represents the intent to modify a <typeparamref name="TEntity" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be modified.</typeparam>
    public interface ICommand<TEntity>
    {
        /// <summary>
        ///     Returns a new set of modifiers for an entity by this command.
        /// </summary>
        /// <param name="entity">The entity to be modified.</param>
        /// <returns>A new set of modifiers for <paramref name="entity" /> by this command.</returns>
        IEnumerable<IFact<TEntity>> Execute(TEntity entity);
    }
}

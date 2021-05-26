using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
using System.Collections.Generic;

namespace EntityDb.Common.Extensions
{
    /// <summary>
    /// Extension methods for entities.
    /// </summary>
    public static class EntityExtensions
    {
        /// <summary>
        /// Returns a new <typeparamref name="TEntity"/> that incorporates the modifications of a set of facts into an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be modified.</typeparam>
        /// <param name="facts">The facts that modify the entity.</param>
        /// <param name="entity">The entity to be modified.</param>
        /// <returns>A new <typeparamref name="TEntity"/> that incorporates the modifications of <paramref name="facts"/> into <paramref name="entity"/>.</returns>
        public static TEntity Reduce<TEntity>(this IEnumerable<IFact<TEntity>> facts, TEntity entity)
        {
            foreach (var fact in facts)
            {
                entity = fact.Reduce(entity);
            }

            return entity;
        }

        /// <summary>
        /// Returns a new instance of <typeparamref name="TEntity"/> that incorporates the modification of a command into an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be modified.</typeparam>
        /// <param name="entity">The entity to be modified.</param>
        /// <param name="command">The command to execute.</param>
        /// <returns>A new instace of <typeparamref name="TEntity"/> that incorporates the modification of <paramref name="command"/> into <paramref name="entity"/>.</returns>
        /// <remarks>
        /// This method is ONLY intended to be used for business logic tests.
        /// </remarks>
        public static TEntity ExecuteAndReduce<TEntity>(this TEntity entity, ICommand<TEntity> command)
        {
            return command.Execute(entity).Reduce(entity);
        }

        /// <summary>
        /// Returns a new instace of <typeparamref name="TEntity"/> that incorporates the modifications of a set of commands into an entity.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity to be modified.</typeparam>
        /// <param name="entity">The entity to be modified.</param>
        /// <param name="commands">The set of commands to execute.</param>
        /// <returns>A new instace of <typeparamref name="TEntity"/> that incorporates the modifications of <paramref name="commands"/> into <paramref name="entity"/>.</returns>
        /// <remarks>
        /// This method is ONLY intended to be used for business logic tests.
        /// </remarks>
        public static TEntity ExecuteAndReduce<TEntity>(this TEntity entity, IEnumerable<ICommand<TEntity>> commands)
        {
            foreach (var command in commands)
            {
                entity = entity.ExecuteAndReduce(command);
            }

            return entity;
        }
    }
}

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
    }
}

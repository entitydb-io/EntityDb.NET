using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Strategies;

namespace EntityDb.Common.Entities
{
    /// <summary>
    ///     Represents a type that can be used for an implementation of <see cref="IAuthorizingStrategy{TEntity}" />.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be authorized.</typeparam>
    public interface IAuthorizedEntity<TEntity>
    {
        /// <inheritdoc cref="IAuthorizingStrategy{TEntity}.IsAuthorized(TEntity, ICommand{TEntity}, IAgent)" />
        bool IsAuthorized(ICommand<TEntity> command, IAgent agent);
    }
}

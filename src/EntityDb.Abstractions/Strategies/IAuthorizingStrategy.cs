using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;

namespace EntityDb.Abstractions.Strategies
{
    /// <summary>
    /// Represents a type used to determine if an agent is authorized to execute a command on an entity.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to be authorized.</typeparam>
    public interface IAuthorizingStrategy<TEntity>
    {
        /// <summary>
        /// Determines if the agent is authorized to execute a command on an entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <param name="command">The command.</param>
        /// <param name="agent">The agent.</param>
        /// <returns><c>true</c> if execution is authorized, or <c>false</c> if execution is not authorized.</returns>
        bool IsAuthorized(TEntity entity, ICommand<TEntity> command, IAgent agent);
    }
}

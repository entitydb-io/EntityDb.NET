using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Entities;

namespace EntityDb.Common.Strategies
{
    internal sealed class AuthorizedEntityAuthorizingStrategy<TEntity> : IAuthorizingStrategy<TEntity>
        where TEntity : IAuthorizedEntity<TEntity>
    {
        public bool IsAuthorized(TEntity entity, ICommand<TEntity> command, IAgent agent)
        {
            return entity.IsAuthorized(command, agent);
        }
    }
}

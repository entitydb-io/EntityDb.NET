using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Entities;

namespace EntityDb.Common.Strategies
{
    internal sealed class AuthorizedEntityAuthorizingStrategy<TEntity> : IAuthorizingStrategy<TEntity>
        where TEntity : IAuthorizedEntity<TEntity>
    {
        private readonly IAgentAccessor _agentAccessor;

        public AuthorizedEntityAuthorizingStrategy(IAgentAccessor agentAccessor)
        {
            _agentAccessor = agentAccessor;
        }

        public bool IsAuthorized(TEntity entity, ICommand<TEntity> command)
        {
            var agent = _agentAccessor.GetAgent();
            
            return entity.IsAuthorized(command, agent);
        }
    }
}

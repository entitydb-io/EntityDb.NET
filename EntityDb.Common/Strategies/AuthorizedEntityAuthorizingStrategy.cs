using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Strategies;
using EntityDb.Common.Entities;
using System.Security.Claims;

namespace EntityDb.Common.Strategies
{
    internal sealed class AuthorizedEntityAuthorizingStrategy<TEntity> : IAuthorizingStrategy<TEntity>
        where TEntity : IAuthorizedEntity<TEntity>
    {
        public bool IsAuthorized(TEntity entity, ICommand<TEntity> command, ClaimsPrincipal claimsPrincipal)
        {
            return entity.IsAuthorized(command, claimsPrincipal);
        }
    }
}

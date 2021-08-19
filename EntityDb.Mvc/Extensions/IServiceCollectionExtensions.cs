using EntityDb.Abstractions.Agents;
using EntityDb.Common.Extensions;
using EntityDb.Mvc.Agents;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Mvc.Extensions
{
    /// <inheritdoc cref="Common.Extensions.IServiceCollectionExtensions"/>
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an internal implementation of <see cref="IAgentAccessor"/> which spawns an agent from the HttpContext.
        /// </summary>
        /// <param name="serviceCollection">The service collection</param>
        public static void AddHttpContextAgentAccessor(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddAgentAccessor<HttpContextAgentAccessor>();
        }
    }
}

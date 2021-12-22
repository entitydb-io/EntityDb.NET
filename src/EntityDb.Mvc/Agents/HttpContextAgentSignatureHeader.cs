using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace EntityDb.Mvc.Agents
{
    /// <summary>
    ///     Represents the headers used by agent to request a transaction using a
    ///     <see cref="Controller" />.
    /// </summary>
    public sealed record HttpContextAgentSignatureHeader(string Name, string[] Values)
    {
        /// <summary>
        ///     Returns a new instance of <see cref="HttpContextAgentSignatureHeader" /> that is mapped from an <see cref="HttpContext" />.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext" />.</param>
        /// <returns>A new instance of <see cref="HttpContextAgentSignatureHeader" /> that is mapped from <paramref name="httpContext" />.</returns>
        public static HttpContextAgentSignatureHeader[] FromHttpContext(HttpContext httpContext)
        {
            return httpContext.Request.Headers
                .Select(pair => new HttpContextAgentSignatureHeader
                (
                    pair.Key,
                    pair.Value.ToArray()
                ))
                .ToArray();
        }
    }
}

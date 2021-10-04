using Microsoft.AspNetCore.Http;
using System.Linq;

namespace EntityDb.Mvc.Sources
{
    /// <summary>
    ///     Represents the headers used by agent to request a transaction using a
    ///     <see cref="Microsoft.AspNetCore.Mvc.Controller" />.
    /// </summary>
    public sealed record MvcSourceHeader(string Name, string[] Values)
    {
        /// <summary>
        ///     Returns a new instance of <see cref="MvcSourceHeader" /> that is mapped from an <see cref="HttpContext" />.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext" />.</param>
        /// <returns>A new instance of <see cref="MvcSourceHeader" /> that is mapped from <paramref name="httpContext" />.</returns>
        public static MvcSourceHeader[] FromHttpContext(HttpContext httpContext)
        {
            return httpContext.Request.Headers
                .Select(pair => new MvcSourceHeader
                (
                    pair.Key,
                    pair.Value.ToArray()
                ))
                .ToArray();
        }
    }
}

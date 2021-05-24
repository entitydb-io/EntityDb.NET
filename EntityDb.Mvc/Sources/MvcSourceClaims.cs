using Microsoft.AspNetCore.Http;
using System.Linq;

namespace EntityDb.Mvc.Sources
{
    /// <summary>
    /// Represents the claims used by agent to request a transaction using a <see cref="Microsoft.AspNetCore.Mvc.Controller"/>.
    /// </summary>
    public sealed record MvcSourceClaim(string Type, string Value, string? ValueType, string Issuer, string OriginalIssuer)
    {
        /// <summary>
        /// Returns a new array of <see cref="MvcSourceClaim"/> instances that are mapped from an <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <returns>A new array of <see cref="MvcSourceClaim"/> instances that are mapped from <paramref name="httpContext"/>.</returns>
        public static MvcSourceClaim[] FromHttpContext(HttpContext httpContext)
        {
            return httpContext.User.Claims
                .Select(claim => new MvcSourceClaim
                (
                    claim.Type,
                    claim.Value,
                    claim.ValueType,
                    claim.Issuer,
                    claim.OriginalIssuer
                ))
                .ToArray();
        }
    }
}

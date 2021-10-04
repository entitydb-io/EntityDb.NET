using Microsoft.AspNetCore.Http;

namespace EntityDb.Mvc.Sources
{
    /// <summary>
    /// Represents the description of an agent who requests transactions using a <see cref="Microsoft.AspNetCore.Mvc.Controller"/>.
    /// </summary>
    public sealed record MvcSource
    {
        /// <summary>
        /// HTTP Headers
        /// </summary>
        public MvcSourceHeader[] Headers { get; init; } = default!;

        /// <summary>
        /// HTTP Connection
        /// </summary>
        public MvcSourceConnection Connection { get; init; } = default!;

        /// <summary>
        /// Returns a new instance of <see cref="MvcSource"/> that is mapped from an <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext"/>.</param>
        /// <returns>A new instance of <see cref="MvcSource"/> that is mapped from <paramref name="httpContext"/>.</returns>
        public static MvcSource FromHttpContext(HttpContext httpContext)
        {
            var headers = MvcSourceHeader.FromHttpContext(httpContext);
            var connection = MvcSourceConnection.FromHttpContext(httpContext);

            return new MvcSource
            {
                Headers = headers,
                Connection = connection,
            };
        }
    }
}

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EntityDb.Mvc.Agents
{
    /// <summary>
    ///     Represents the description of an agent who requests transactions using a
    ///     <see cref="Controller" />.
    /// </summary>
    public sealed record HttpContextAgentSignature
    {
        /// <summary>
        ///     HTTP Headers
        /// </summary>
        public HttpContextAgentSignatureHeader[] Headers { get; init; } = default!;

        /// <summary>
        ///     HTTP Connection
        /// </summary>
        public HttpContextAgentSignatureConnection Connection { get; init; } = default!;

        /// <summary>
        ///     Returns a new instance of <see cref="HttpContextAgentSignature" /> that is mapped from an <see cref="HttpContext" />.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext" />.</param>
        /// <returns>A new instance of <see cref="HttpContextAgentSignature" /> that is mapped from <paramref name="httpContext" />.</returns>
        public static HttpContextAgentSignature FromHttpContext(HttpContext httpContext)
        {
            var headers = HttpContextAgentSignatureHeader.FromHttpContext(httpContext);
            var connection = HttpContextAgentSignatureConnection.FromHttpContext(httpContext);

            return new HttpContextAgentSignature { Headers = headers, Connection = connection };
        }
    }
}

using EntityDb.Mvc.Agents;
using EntityDb.Mvc.Tests.Seeder;
using Shouldly;
using System;
using Xunit;

namespace EntityDb.Mvc.Tests.Agents
{
    public class HttpContextAgentSignatureTests
    {
        [Fact]
        public void GivenNoRedactedHeaders_WhenHttpContextHasHeader_ThenAgentSignatureHasHeaderValue()
        {
            // ARRANGE

            const string HeaderName = nameof(HeaderName);
            const string HeaderValue = nameof(HeaderValue);

            var httpContextAgentOptions = new HttpContextAgentSignatureOptions
            {
                RedactedHeaders = Array.Empty<string>()
            };

            var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
            {
                Headers = new()
                {
                    [HeaderName] = new[] { HeaderValue }
                }
            });

            // ACT

            var agentSignature = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions);

            // ASSERT

            agentSignature.Request.Headers.Length.ShouldBe(1);
            agentSignature.Request.Headers[0].Name.ShouldBe(HeaderName);
            agentSignature.Request.Headers[0].Values.Length.ShouldBe(1);
            agentSignature.Request.Headers[0].Values[0].ShouldBe(HeaderValue);
        }

        [Fact]
        public void GivenRedactedHeader_WhenHttpContextContainsOnlyThatHeader_ThenAgentSignatureHasRedactedHeader()
        {
            // ARRANGE

            const string HeaderName = nameof(HeaderName);
            const string HeaderValue = nameof(HeaderValue);
            const string RedactedValue = nameof(RedactedValue);

            var httpContextAgentOptions = new HttpContextAgentSignatureOptions
            {
                RedactedHeaders = new[] { HeaderName },
                RedactedValue = RedactedValue,
            };

            var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
            {
                Headers = new()
                {
                    [HeaderName] = new[] { HeaderValue }
                }
            });

            // ACT

            var agentSignature = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions);

            // ASSERT

            agentSignature.Request.Headers.Length.ShouldBe(1);
            agentSignature.Request.Headers[0].Name.ShouldBe(HeaderName);
            agentSignature.Request.Headers[0].Values.Length.ShouldBe(1);
            agentSignature.Request.Headers[0].Values[0].ShouldBe(RedactedValue);
        }

        [Fact]
        public void GivenNoRedactedQueryStringParams_WhenHttpContextHasQueryStringParam_ThenAgentSignatureHasQueryStringParamValue()
        {
            // ARRANGE

            const string QueryStringParamName = nameof(QueryStringParamName);
            const string QueryStringParamValue = nameof(QueryStringParamValue);

            var httpContextAgentOptions = new HttpContextAgentSignatureOptions
            {
                RedactedQueryStringParams = Array.Empty<string>()
            };

            var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
            {
                QueryStringParams = new()
                {
                    [QueryStringParamName] = new[] { QueryStringParamValue }
                }
            });

            // ACT

            var agentSignature = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions);

            // ASSERT

            agentSignature.Request.QueryStringParams.Length.ShouldBe(1);
            agentSignature.Request.QueryStringParams[0].Name.ShouldBe(QueryStringParamName);
            agentSignature.Request.QueryStringParams[0].Values.Length.ShouldBe(1);
            agentSignature.Request.QueryStringParams[0].Values[0].ShouldBe(QueryStringParamValue);
        }

        [Fact]
        public void GivenRedactedQueryStringParam_WhenHttpContextContainsOnlyThatQueryStringParam_ThenAgentSignatureHasRedactedQueryStringParams()
        {
            // ARRANGE

            const string QueryStringParamName = nameof(QueryStringParamName);
            const string QueryStringParamValue = nameof(QueryStringParamValue);
            const string RedactedValue = nameof(RedactedValue);

            var httpContextAgentOptions = new HttpContextAgentSignatureOptions
            {
                RedactedQueryStringParams = new[] { QueryStringParamName },
                RedactedValue = RedactedValue,
            };

            var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
            {
                QueryStringParams = new()
                {
                    [QueryStringParamName] = new[] { QueryStringParamValue }
                }
            });

            // ACT

            var agentSignature = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions);

            // ASSERT

            agentSignature.Request.QueryStringParams.Length.ShouldBe(1);
            agentSignature.Request.QueryStringParams[0].Name.ShouldBe(QueryStringParamName);
            agentSignature.Request.QueryStringParams[0].Values.Length.ShouldBe(1);
            agentSignature.Request.QueryStringParams[0].Values[0].ShouldBe(RedactedValue);
        }
    }
}

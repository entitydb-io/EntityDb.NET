using EntityDb.Mvc.Agents;
using EntityDb.Mvc.Tests.Seeder;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace EntityDb.Mvc.Tests.Agents;

public class HttpContextAgentSignatureTests
{
    [Fact]
    public void GivenNoRedactedHeaders_WhenHttpContextHasHeader_ThenAgentSignatureHasHeaderValue()
    {
        // ARRANGE

        const string headerName = nameof(headerName);
        const string headerValue = nameof(headerValue);

        var httpContextAgentOptions = new HttpContextAgentSignatureOptions
        {
            RedactedHeaders = Array.Empty<string>()
        };

        var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
        {
            Headers = new Dictionary<string, string[]>
            {
                [headerName] = new[] { headerValue }
            }
        });

        // ACT

        var (request, _, _) = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions, default);

        // ASSERT

        request.Headers.Length.ShouldBe(1);
        request.Headers[0].Name.ShouldBe(headerName);
        request.Headers[0].Values.Length.ShouldBe(1);
        request.Headers[0].Values[0].ShouldBe(headerValue);
    }

    [Fact]
    public void GivenRedactedHeader_WhenHttpContextContainsOnlyThatHeader_ThenAgentSignatureHasRedactedHeader()
    {
        // ARRANGE

        const string headerName = nameof(headerName);
        const string headerValue = nameof(headerValue);
        const string redactedValue = nameof(redactedValue);

        var httpContextAgentOptions = new HttpContextAgentSignatureOptions
        {
            RedactedHeaders = new[] { headerName },
            RedactedValue = redactedValue
        };

        var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
        {
            Headers = new Dictionary<string, string[]>
            {
                [headerName] = new[] { headerValue }
            }
        });

        // ACT

        var (request, _, _) = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions, default);

        // ASSERT

        request.Headers.Length.ShouldBe(1);
        request.Headers[0].Name.ShouldBe(headerName);
        request.Headers[0].Values.Length.ShouldBe(1);
        request.Headers[0].Values[0].ShouldBe(redactedValue);
    }

    [Fact]
    public void GivenNoRedactedQueryStringParams_WhenHttpContextHasQueryStringParam_ThenAgentSignatureHasQueryStringParamValue()
    {
        // ARRANGE

        const string queryStringParamName = nameof(queryStringParamName);
        const string queryStringParamValue = nameof(queryStringParamValue);

        var httpContextAgentOptions = new HttpContextAgentSignatureOptions
        {
            RedactedQueryStringParams = Array.Empty<string>()
        };

        var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
        {
            QueryStringParams = new Dictionary<string, string[]>
            {
                [queryStringParamName] = new[] { queryStringParamValue }
            }
        });

        // ACT

        var (request, _, _) = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions, default);

        // ASSERT

        request.QueryStringParams.Length.ShouldBe(1);
        request.QueryStringParams[0].Name.ShouldBe(queryStringParamName);
        request.QueryStringParams[0].Values.Length.ShouldBe(1);
        request.QueryStringParams[0].Values[0].ShouldBe(queryStringParamValue);
    }

    [Fact]
    public void GivenRedactedQueryStringParam_WhenHttpContextContainsOnlyThatQueryStringParam_ThenAgentSignatureHasRedactedQueryStringParams()
    {
        // ARRANGE

        const string queryStringParamName = nameof(queryStringParamName);
        const string queryStringParamValue = nameof(queryStringParamValue);
        const string redactedValue = nameof(redactedValue);

        var httpContextAgentOptions = new HttpContextAgentSignatureOptions
        {
            RedactedQueryStringParams = new[] { queryStringParamName },
            RedactedValue = redactedValue
        };

        var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
        {
            QueryStringParams = new Dictionary<string, string[]>
            {
                [queryStringParamName] = new[] { queryStringParamValue }
            }
        });

        // ACT

        var (request, _, _) = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions, default);

        // ASSERT

        request.QueryStringParams.Length.ShouldBe(1);
        request.QueryStringParams[0].Name.ShouldBe(queryStringParamName);
        request.QueryStringParams[0].Values.Length.ShouldBe(1);
        request.QueryStringParams[0].Values[0].ShouldBe(redactedValue);
    }
}
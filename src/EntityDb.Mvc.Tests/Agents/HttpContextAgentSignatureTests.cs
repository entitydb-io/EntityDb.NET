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
        public void GivenNoDoNotRecordHeaders_WhenHttpContextHasSingleHeader_ThenAgentSignatureHasSingleHeader()
        {
            // ARRANGE

            const string RecordHeaderName = nameof(RecordHeaderName);
            const string RecordHeaderValue = nameof(RecordHeaderValue);

            var httpContextAgentOptions = new HttpContextAgentSignatureOptions
            {
                DoNotRecordHeaders = Array.Empty<string>()
            };

            var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
            {
                Headers = new()
                {
                    [RecordHeaderName] = new[] { RecordHeaderValue }
                }
            });

            // ACT

            var agentSignature = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions);

            // ASSERT

            agentSignature.Request.Headers.Length.ShouldBe(1);
            agentSignature.Request.Headers[0].Name.ShouldBe(RecordHeaderName);
            agentSignature.Request.Headers[0].Values.Length.ShouldBe(1);
            agentSignature.Request.Headers[0].Values[0].ShouldBe(RecordHeaderValue);
        }

        [Fact]
        public void GivenDoNotRecordHeader_WhenHttpContextContainsOnlyThatHeader_ThenAgentSignatureHasNoHeaders()
        {
            // ARRANGE

            const string DoNotRecordHeader = nameof(DoNotRecordHeader);

            var httpContextAgentOptions = new HttpContextAgentSignatureOptions
            {
                DoNotRecordHeaders = new[] { DoNotRecordHeader }
            };

            var httpContext = HttpContextSeeder.CreateHttpContext(new HttpContextSeederOptions
            {
                Headers = new()
                {
                    [DoNotRecordHeader] = Array.Empty<string>()
                }
            });

            // ACT

            var agentSignature = HttpContextAgentSignature.GetSnapshot(httpContext, httpContextAgentOptions);

            // ASSERT

            agentSignature.Request.Headers.ShouldBeEmpty();
        }
    }
}

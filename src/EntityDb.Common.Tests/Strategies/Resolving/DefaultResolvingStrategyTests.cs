using EntityDb.Common.Envelopes;
using EntityDb.Common.Strategies.Resolving;
using Shouldly;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace EntityDb.Common.Tests.Strategies.Resolving
{
    public class DefaultResolvingStrategyTests
    {
        [Fact]
        public void GivenFullNames_WhenLoadingType_ThenReturnType()
        {
            // ARRANGE

            var record = new object();

            var expectedType = record.GetType();

            var headers = EnvelopeHelper.GetTypeHeaders(expectedType, includeFullNames: true, includeMemberInfoName: false);

            var resolvingStrategy = new DefaultResolvingStrategy();

            // ACT

            var actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBe(expectedType);
        }

        [Fact]
        public void GivenMemberInfoName_WhenLoadingType_ThenReturnNull()
        {
            // ARRANGE

            var headers = EnvelopeHelper.GetTypeHeaders(typeof(object), includeFullNames: false, includeMemberInfoName: true);

            var resolvingStrategy = new DefaultResolvingStrategy();

            // ACT

            var actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBeNull();
        }

        [Fact]
        public void GivenNoTypeInformation_WhenLoadingType_ThenReturnNull()
        {
            // ARRANGE

            var resolvingStrategy = new DefaultResolvingStrategy();

            var headers = new Dictionary<string, string>
            {
                [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform,
            };

            // ACT

            var actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBeNull();
        }

        [Fact]
        public void GivenGarbageTypeInformation_WhenLoadingType_ThenThrow()
        {
            // ARRANGE

            var resolvingStrategy = new DefaultResolvingStrategy();

            var headers = new Dictionary<string, string>
            {
                [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform,
                [EnvelopeHelper.AssemblyFullName] = "Garbage",
                [EnvelopeHelper.TypeFullName] = "Garbage",
                [EnvelopeHelper.MemberInfoName] = "Garbage",
            };

            // ASSERT

            Should.Throw<FileNotFoundException>(() => resolvingStrategy.ResolveType(headers));
        }
    }
}

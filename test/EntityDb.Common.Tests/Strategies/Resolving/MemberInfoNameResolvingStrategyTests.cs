using EntityDb.Common.Envelopes;
using EntityDb.Common.Strategies.Resolving;
using Shouldly;
using System;
using System.Collections.Generic;
using Xunit;

namespace EntityDb.Common.Tests.Strategies.Resolving
{
    public class MemberInfoNameResolvingStrategyTests
    {
        [Fact]
        public void GivenMemberInfoNameResolvingStrategyKnowsExpectedType_WhenResolvingType_ThenReturnExpectedType()
        {
            // ARRANGE

            var expectedType = typeof(string);

            var headers = new Dictionary<string, string>
            {
                [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform,
                [EnvelopeHelper.MemberInfoName] = expectedType.Name
            };

            var resolvingStrategy = new MemberInfoNameResolvingStrategy(new[] { expectedType });

            // ACT

            var resolved = resolvingStrategy.TryResolveType(headers, out var actualType);

            // ASSERT

            resolved.ShouldBeTrue();
            actualType.ShouldBe(expectedType);
        }

        [Fact]
        public void GivenNonEmptyMemberInfoResolvingStrategy_WhenResolvingTypeWithNoInformation_ThenReturnNull()
        {
            // ARRANGE

            var resolvingStrategy = new MemberInfoNameResolvingStrategy(new[] { typeof(string) });

            var headers = new Dictionary<string, string>();

            // ACT

            var resolved = resolvingStrategy.TryResolveType(headers, out var actualType);

            // ASSERT

            resolved.ShouldBeFalse();
            actualType.ShouldBeNull();
        }

        [Fact]
        public void GivenEmptyMemberInfoNameResolvingStrategy_WhenResolvingType_ThenReturnNull()
        {
            // ARRANGE

            var resolvingStrategy = new MemberInfoNameResolvingStrategy(Array.Empty<Type>());

            var headers = new Dictionary<string, string>
            {
                [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform, [EnvelopeHelper.MemberInfoName] = ""
            };

            // ACT

            var resolved = resolvingStrategy.TryResolveType(headers, out var actualType);

            // ASSERT

            resolved.ShouldBeFalse();
            actualType.ShouldBeNull();
        }
    }
}

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

            Type? expectedType = typeof(string);

            Dictionary<string, string>? headers = new Dictionary<string, string>
            {
                [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform,
                [EnvelopeHelper.MemberInfoName] = expectedType.Name
            };

            MemberInfoNameResolvingStrategy? resolvingStrategy =
                new MemberInfoNameResolvingStrategy(new[] { expectedType });

            // ACT

            Type? actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBe(expectedType);
        }

        [Fact]
        public void GivenNonEmptyMemberInfoResolvingStrategy_WhenResolvingTypeWithNoInformation_ThenReturnNull()
        {
            // ARRANGE

            MemberInfoNameResolvingStrategy? resolvingStrategy =
                new MemberInfoNameResolvingStrategy(new[] { typeof(string) });

            Dictionary<string, string>? headers = new Dictionary<string, string>();

            // ACT

            Type? actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBeNull();
        }

        [Fact]
        public void GivenEmptyMemberInfoNameResolvingStrategy_WhenResolvingType_ThenReturnNull()
        {
            // ARRANGE

            MemberInfoNameResolvingStrategy? resolvingStrategy =
                new MemberInfoNameResolvingStrategy(Array.Empty<Type>());

            Dictionary<string, string>? headers = new Dictionary<string, string>
            {
                [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform, [EnvelopeHelper.MemberInfoName] = ""
            };

            // ACT

            Type? actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBeNull();
        }
    }
}

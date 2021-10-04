using EntityDb.Common.Envelopes;
using EntityDb.Common.Strategies.Resolving;
using Shouldly;
using System;
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

            object? record = new object();

            Type? expectedType = record.GetType();

            Dictionary<string, string>? headers = EnvelopeHelper.GetTypeHeaders(expectedType, true, false);

            DefaultResolvingStrategy? resolvingStrategy = new DefaultResolvingStrategy();

            // ACT

            Type? actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBe(expectedType);
        }

        [Fact]
        public void GivenMemberInfoName_WhenLoadingType_ThenReturnNull()
        {
            // ARRANGE

            Dictionary<string, string>? headers = EnvelopeHelper.GetTypeHeaders(typeof(object), false, true);

            DefaultResolvingStrategy? resolvingStrategy = new DefaultResolvingStrategy();

            // ACT

            Type? actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBeNull();
        }

        [Fact]
        public void GivenNoTypeInformation_WhenLoadingType_ThenReturnNull()
        {
            // ARRANGE

            DefaultResolvingStrategy? resolvingStrategy = new DefaultResolvingStrategy();

            Dictionary<string, string>? headers = new Dictionary<string, string>
            {
                [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform
            };

            // ACT

            Type? actualType = resolvingStrategy.ResolveType(headers);

            // ASSERT

            actualType.ShouldBeNull();
        }

        [Fact]
        public void GivenGarbageTypeInformation_WhenLoadingType_ThenThrow()
        {
            // ARRANGE

            DefaultResolvingStrategy? resolvingStrategy = new DefaultResolvingStrategy();

            Dictionary<string, string>? headers = new Dictionary<string, string>
            {
                [EnvelopeHelper.Platform] = EnvelopeHelper.ThisPlatform,
                [EnvelopeHelper.AssemblyFullName] = "Garbage",
                [EnvelopeHelper.TypeFullName] = "Garbage",
                [EnvelopeHelper.MemberInfoName] = "Garbage"
            };

            // ASSERT

            Should.Throw<FileNotFoundException>(() => resolvingStrategy.ResolveType(headers));
        }
    }
}

using EntityDb.Common.Strategies.Resolving;
using System;
using Xunit;

namespace EntityDb.Common.Tests.Strategies.Resolving
{
    public class TypeNameResolvingStrategyTests
    {
        [Fact]
        public void GivenTypeNameResolvingStrategyKnowsExpectedType_WhenResolvingType_ThenReturnExpectedType()
        {
            // ARRANGE

            var expectedType = typeof(string);

            var resolvingStrategy = new TypeNameResolvingStrategy(new[] { expectedType });

            // ACT

            var actualType = resolvingStrategy.ResolveType(null, null, expectedType.Name);

            // ASSERT

            Assert.Equal(expectedType, actualType);
        }

        [Fact]
        public void GivenNonEmptyTypeNameResolvingStrategy_WhenResolvingTypeWithNoInformation_ThenReturnNull()
        {
            // ARRANGE

            var resolvingStrategy = new TypeNameResolvingStrategy(new[] { typeof(string) });

            // ACT

            var actualType = resolvingStrategy.ResolveType(null, null, null);

            // ASSERT

            Assert.Null(actualType);
        }

        [Fact]
        public void GivenEmptyTypeNameResolvingStrategy_WhenResolvingType_ThenReturnNull()
        {
            // ARRANGE

            var resolvingStrategy = new TypeNameResolvingStrategy(Array.Empty<Type>());

            // ACT

            var actualType = resolvingStrategy.ResolveType(null, null, "");

            // ASSERT

            Assert.Null(actualType);
        }
    }
}

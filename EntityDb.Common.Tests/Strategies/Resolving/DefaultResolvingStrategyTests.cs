using EntityDb.Common.Extensions;
using EntityDb.Common.Strategies.Resolving;
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

            var (assemblyFullName, typeFullName, _) = record.GetType().GetTypeInfo();

            var resolvingStrategy = new DefaultResolvingStrategy();

            // ACT

            var actualType = resolvingStrategy.ResolveType(assemblyFullName, typeFullName, null);

            // ASSERT

            Assert.Equal(expectedType, actualType);
        }

        [Fact]
        public void GivenTypeName_WhenLoadingType_ThenReturnNull()
        {
            // ARRANGE

            var (_, _, typeName) = typeof(object).GetTypeInfo();

            var resolvingStrategy = new DefaultResolvingStrategy();

            // ACT

            var actualType = resolvingStrategy.ResolveType(null, null, typeName);

            // ASSERT

            Assert.Null(actualType);
        }

        [Fact]
        public void GivenNoTypeInformation_WhenLoadingType_ThenReturnNull()
        {
            // ARRANGE

            var resolvingStrategy = new DefaultResolvingStrategy();

            // ACT

            var actualType = resolvingStrategy.ResolveType(null, null, null);

            // ASSERT

            Assert.Null(actualType);
        }

        [Fact]
        public void GivenGarbageTypeInformation_WhenLoadingType_ThenThrow()
        {
            // ARRANGE

            var resolvingStrategy = new DefaultResolvingStrategy();

            // ASSERT

            Assert.Throws<FileNotFoundException>(() =>
            {
                resolvingStrategy.ResolveType("Garbage", "Garbage", "Garbage");
            });
        }
    }
}

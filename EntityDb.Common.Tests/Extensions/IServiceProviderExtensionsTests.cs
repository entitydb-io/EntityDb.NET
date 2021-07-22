using EntityDb.Common.Extensions;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Entities;
using System;
using Xunit;

namespace EntityDb.Common.Tests.Extensions
{
    public class IServiceProviderExtensionsTests : TestsBase
    {
        public IServiceProviderExtensionsTests(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        [Fact]
        public void GivenNoLeasingStrategy_WhenGettingLeases_ThenReturnEmptyArray()
        {
            // ARRANGE

            var serviceProvider = GetEmptyServiceProvider();

            // ACT

            var leases = serviceProvider.GetLeases(new TransactionEntity());

            // ASSERT

            Assert.Empty(leases);
        }

        [Fact]
        public void GivenNoAuthorizingStrategy_ThenIsAuthorizedReturnsTrue()
        {
            // ARRANGE

            var serviceProvider = GetEmptyServiceProvider();

            // ACT

            var isAuthorized = serviceProvider.IsAuthorized(new TransactionEntity(), new DoNothing(), default!);

            // ASSERT

            Assert.True(isAuthorized);
        }

        [Fact]
        public void GivenNoCachingStrategy_WhenCheckingIfShouldCache_ThenReturnFalse()
        {
            // ARRANGE

            var serviceProvider = GetEmptyServiceProvider();

            // ACT

            var shouldCache = serviceProvider.ShouldCache<TransactionEntity>(default, default!);

            // ASSERT

            Assert.False(shouldCache);
        }
    }
}

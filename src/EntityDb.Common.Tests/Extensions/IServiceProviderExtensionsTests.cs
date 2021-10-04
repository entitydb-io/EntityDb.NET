using EntityDb.Common.Extensions;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Entities;
using Shouldly;
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

            leases.ShouldBeEmpty();
        }

        [Fact]
        public void GivenNoAuthorizingStrategy_ThenIsAuthorizedReturnsTrue()
        {
            // ARRANGE

            var serviceProvider = GetEmptyServiceProvider();

            // ACT

            var isAuthorized = serviceProvider.IsAuthorized(new TransactionEntity(), new DoNothing());

            // ASSERT

            isAuthorized.ShouldBeTrue();
        }

        [Fact]
        public void GivenNoCachingStrategy_WhenCheckingIfShouldCache_ThenReturnFalse()
        {
            // ARRANGE

            var serviceProvider = GetEmptyServiceProvider();

            // ACT

            var shouldCache = serviceProvider.ShouldPutSnapshot<TransactionEntity>(default, default!);

            // ASSERT

            shouldCache.ShouldBeFalse();
        }
    }
}

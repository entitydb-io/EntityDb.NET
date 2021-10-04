using EntityDb.Abstractions.Leases;
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

            IServiceProvider? serviceProvider = GetEmptyServiceProvider();

            // ACT

            ILease[]? leases = serviceProvider.GetLeases(new TransactionEntity());

            // ASSERT

            leases.ShouldBeEmpty();
        }

        [Fact]
        public void GivenNoAuthorizingStrategy_ThenIsAuthorizedReturnsTrue()
        {
            // ARRANGE

            IServiceProvider? serviceProvider = GetEmptyServiceProvider();

            // ACT

            bool isAuthorized = serviceProvider.IsAuthorized(new TransactionEntity(), new DoNothing());

            // ASSERT

            isAuthorized.ShouldBeTrue();
        }

        [Fact]
        public void GivenNoCachingStrategy_WhenCheckingIfShouldCache_ThenReturnFalse()
        {
            // ARRANGE

            IServiceProvider? serviceProvider = GetEmptyServiceProvider();

            // ACT

            bool shouldCache = serviceProvider.ShouldPutSnapshot<TransactionEntity>(default, default!);

            // ASSERT

            shouldCache.ShouldBeFalse();
        }
    }
}

using EntityDb.Abstractions.Entities;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Transactions.Builders;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System;
using System.Threading.Tasks;
using EntityDb.Abstractions.ValueObjects;
using Xunit;

namespace EntityDb.Common.Tests.Entities;

public abstract class EntityTestsBase<TStartup> : TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    protected EntityTestsBase(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

    private static ITransaction BuildTransaction
    (
        IServiceScope serviceScope,
        Id entityId,
        VersionNumber from,
        VersionNumber to,
        TransactionEntity? entity = null
    )
    {
        var transactionBuilder = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(entityId);

        if (entity != null)
        {
            transactionBuilder.Load(entity);
        }

        for (var i = from; i.Value <= to.Value; i = i.Next())
        {
            if (transactionBuilder.IsEntityKnown() && transactionBuilder.GetEntity().VersionNumber.Value >= i.Value)
            {
                continue;
            }

            transactionBuilder.Append(new DoNothing());
        }

        return transactionBuilder.Build(default!, Id.NewId());
    }

    [Theory]
    [InlineData(10UL, 5UL)]
    public async Task GivenEntityWithNVersions_WhenGettingAtVersionM_ThenReturnAtVersionM(ulong n, ulong m)
    {
        // ARRANGE

        var versionNumberN = new VersionNumber(n);

        var versionNumberM = new VersionNumber(m);
        
        using var serviceScope = CreateServiceScope();

        var entityId = Id.NewId();

        await using var entityRepository = await serviceScope.ServiceProvider
            .GetRequiredService<IEntityRepositoryFactory<TransactionEntity>>()
            .CreateRepository(TestSessionOptions.Write,
                TestSessionOptions.Write);

        var transaction = BuildTransaction(serviceScope, entityId, new VersionNumber(1), versionNumberN);

        var transactionInserted = await entityRepository.PutTransaction(transaction);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var entityAtVersionM = await entityRepository.GetAtVersion(entityId, versionNumberM);

        // ASSERT

        entityAtVersionM.VersionNumber.ShouldBe(versionNumberM);
    }
}
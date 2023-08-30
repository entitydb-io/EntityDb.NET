using System.Collections.Immutable;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Agents;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Leases;
using EntityDb.Common.Queries;
using EntityDb.Common.Queries.Modified;
using EntityDb.Common.Tags;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Queries;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Snapshots;
using EntityDb.Common.Tests.Implementations.Tags;
using EntityDb.Common.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace EntityDb.Common.Tests.Transactions;

[Collection(nameof(DatabaseContainerCollection))]
public sealed class TransactionTests : TestsBase<Startup>
{
    public TransactionTests(IServiceProvider startupServiceProvider, DatabaseContainerFixture databaseContainerFixture) : base(startupServiceProvider, databaseContainerFixture)
    {
    }

    private static async Task InsertTransactions
    (
        IServiceScope serviceScope,
        List<ITransaction> transactions
    )
    {
        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        foreach (var transaction in transactions)
        {
            var transactionInserted = await transactionRepository.PutTransaction(transaction);

            transactionInserted.ShouldBeTrue();
        }
    }

    private static ModifiedQueryOptions NewModifiedQueryOptions(bool invertFilter, bool reverseSort, int? replaceSkip,
        int? replaceTake)
    {
        return new ModifiedQueryOptions
        {
            InvertFilter = invertFilter,
            ReverseSort = reverseSort,
            ReplaceSkip = replaceSkip,
            ReplaceTake = replaceTake
        };
    }

    private static async Task TestGet<TResult>
    (
        IServiceScope serviceScope,
        Func<bool, TResult[]> getExpectedResults,
        Func<ITransactionRepository, ModifiedQueryOptions, IAsyncEnumerable<TResult>> getActualResults,
        bool secondaryPreferred
    )
    {
        // ARRANGE

        var bufferModifier = NewModifiedQueryOptions(false, false, null, null);
        var negateModifier = NewModifiedQueryOptions(true, false, null, null);
        var reverseBufferModifier = NewModifiedQueryOptions(false, true, null, null);
        var reverseNegateModifier = NewModifiedQueryOptions(true, true, null, null);
        var bufferSubsetModifier = NewModifiedQueryOptions(false, false, 1, 1);

        var expectedTrueResults = getExpectedResults.Invoke(false);
        var expectedFalseResults = getExpectedResults.Invoke(true);
        var reversedExpectedTrueResults = expectedTrueResults.Reverse().ToArray();
        var reversedExpectedFalseResults = expectedFalseResults.Reverse().ToArray();
        var expectedSkipTakeResults = expectedTrueResults.Skip(1).Take(1).ToArray();

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(secondaryPreferred
                ? TestSessionOptions.ReadOnlySecondaryPreferred
                : TestSessionOptions.ReadOnly);

        // ACT

        var actualTrueResults =
            await getActualResults.Invoke(transactionRepository, bufferModifier).ToArrayAsync();
        var actualFalseResults =
            await getActualResults.Invoke(transactionRepository, negateModifier).ToArrayAsync();
        var reversedActualTrueResults =
            await getActualResults.Invoke(transactionRepository, reverseBufferModifier).ToArrayAsync();
        var reversedActualFalseResults =
            await getActualResults.Invoke(transactionRepository, reverseNegateModifier).ToArrayAsync();
        var actualSkipTakeResults =
            await getActualResults.Invoke(transactionRepository, bufferSubsetModifier).ToArrayAsync();

        // ASSERT

        actualTrueResults.ShouldBeEquivalentTo(expectedTrueResults);
        actualFalseResults.ShouldBeEquivalentTo(expectedFalseResults);
        reversedActualTrueResults.ShouldBeEquivalentTo(reversedExpectedTrueResults);
        reversedActualFalseResults.ShouldBeEquivalentTo(reversedExpectedFalseResults);
        actualSkipTakeResults.ShouldBeEquivalentTo(expectedSkipTakeResults);
    }

    private static async Task TestGetTransactionIds
    (
        IServiceScope serviceScope,
        IAgentSignatureQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTransactionIds
                    : expectedObjects.TrueTransactionIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateTransactionIds(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetTransactionIds
    (
        IServiceScope serviceScope,
        ICommandQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTransactionIds
                    : expectedObjects.TrueTransactionIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateTransactionIds(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetTransactionIds
    (
        IServiceScope serviceScope,
        ILeaseQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTransactionIds
                    : expectedObjects.TrueTransactionIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateTransactionIds(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetTransactionIds
    (
        IServiceScope serviceScope,
        ITagQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTransactionIds
                    : expectedObjects.TrueTransactionIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateTransactionIds(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetEntityIds
    (
        IServiceScope serviceScope,
        IAgentSignatureQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateEntityIds(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetEntityIds
    (
        IServiceScope serviceScope,
        ICommandQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateEntityIds(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetEntityIds
    (
        IServiceScope serviceScope,
        ILeaseQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateEntityIds(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetEntityIds
    (
        IServiceScope serviceScope,
        ITagQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Id[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        IAsyncEnumerable<Id> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateEntityIds(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetAgentSignatures
    (
        IServiceScope serviceScope,
        IAgentSignatureQuery query,
        ExpectedObjects expectedObjects
    )
    {
        object[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseAgentSignatures
                    : expectedObjects.TrueAgentSignatures)
                .ToArray();
        }

        IAsyncEnumerable<object> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateAgentSignatures(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetCommands
    (
        IServiceScope serviceScope,
        ICommandQuery query,
        ExpectedObjects expectedObjects
    )
    {
        object[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseCommands
                    : expectedObjects.TrueCommands)
                .ToArray();
        }

        IAsyncEnumerable<object> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateCommands(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetLeases
    (
        IServiceScope serviceScope,
        ILeaseQuery query,
        ExpectedObjects expectedObjects
    )
    {
        ILease[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseLeases
                    : expectedObjects.TrueLeases)
                .ToArray();
        }

        IAsyncEnumerable<ILease> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateLeases(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task TestGetTags
    (
        IServiceScope serviceScope,
        ITagQuery query,
        ExpectedObjects expectedObjects
    )
    {
        ITag[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTags
                    : expectedObjects.TrueTags)
                .ToArray();
        }

        IAsyncEnumerable<ITag> GetActualResults(ITransactionRepository transactionRepository,
            ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.EnumerateTags(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static async Task<ITransaction> BuildTransaction<TEntity>
    (
        IServiceScope serviceScope,
        Id transactionId,
        Id entityId,
        IEnumerable<ulong> counts,
        TimeStamp? timeStampOverride = null,
        object? agentSignatureOverride = null
    )
        where TEntity : IEntity<TEntity>
    {
        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        foreach (var count in counts)
        {
            transactionBuilder.Append(new StoreNumber(count));
            transactionBuilder.Add(new CountLease(count));
            transactionBuilder.Add(new CountTag(count));
        }

        var transaction = (transactionBuilder.Build(transactionId) as Transaction).ShouldNotBeNull();

        if (timeStampOverride.HasValue)
            transaction = transaction with
            {
                TimeStamp = timeStampOverride.Value
            };

        if (agentSignatureOverride is not null)
            transaction = transaction with
            {
                AgentSignature = agentSignatureOverride
            };

        return transaction;
    }

    private static Id[] GetSortedIds(int numberOfIds)
    {
        return Enumerable
            .Range(1, numberOfIds)
            .Select(_ => Id.NewId())
            .OrderBy(id => id.Value)
            .ToArray();
    }

    private async Task
        Generic_GivenReadOnlyMode_WhenPuttingTransaction_ThenCannotWriteInReadOnlyModeExceptionIsLogged<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<CannotWriteInReadOnlyModeException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var transaction = transactionBuilder
            .Append(CommandSeeder.Create())
            .Build(default);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        // ACT

        var inserted = await transactionRepository.PutTransaction(transaction);

        // ASSERT

        inserted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }

    private async Task Generic_GivenNonUniqueTransactionIds_WhenPuttingTransactions_ThenSecondPutReturnsFalse<TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var transactionId = Id.NewId();

        var firstTransactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var secondTransactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var firstTransaction = firstTransactionBuilder
            .Append(CommandSeeder.Create())
            .Build(transactionId);

        var secondTransaction = secondTransactionBuilder
            .Append(CommandSeeder.Create())
            .Build(transactionId);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var firstTransactionInserted = await transactionRepository.PutTransaction(firstTransaction);
        var secondTransactionInserted = await transactionRepository.PutTransaction(secondTransaction);

        // ASSERT

        firstTransactionInserted.ShouldBeTrue();
        secondTransactionInserted.ShouldBeFalse();
    }

    private async Task Generic_GivenNonUniqueVersionNumbers_WhenInsertingCommands_ThenReturnFalse<TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        const int repeatCount = 2;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var transaction = (transactionBuilder
                .Append(CommandSeeder.Create())
                .Build(default)
            as Transaction)!;

        transaction = transaction with
        {
            Steps = Enumerable
                .Repeat(transaction.Steps, repeatCount)
                .SelectMany(steps => steps)
                .ToImmutableArray()
        };

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ARRANGE ASSERTIONS

        repeatCount.ShouldBeGreaterThan(1);

        // ACT

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        // ASSERT

        transactionInserted.ShouldBeFalse();
    }

    private async Task
        Generic_GivenVersionNumberZero_WhenInsertingCommands_ThenVersionZeroReservedExceptionIsLogged<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var versionNumber = new VersionNumber(0);

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<VersionZeroReservedException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var transactionStepMock = new Mock<IAppendCommandTransactionStep>(MockBehavior.Strict);

        transactionStepMock
            .SetupGet(step => step.EntityId)
            .Returns(default(Id));

        transactionStepMock
            .SetupGet(step => step.PreviousEntityVersionNumber)
            .Returns(versionNumber);

        transactionStepMock
            .SetupGet(step => step.EntityVersionNumber)
            .Returns(versionNumber);

        var transaction = TransactionSeeder.Create(transactionStepMock.Object, transactionStepMock.Object);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var transactionInserted =
            await transactionRepository.PutTransaction(transaction);

        // ASSERT

        transactionInserted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }

    private async Task
        Generic_GivenNonUniqueVersionNumbers_WhenInsertingCommands_ThenOptimisticConcurrencyExceptionIsLogged<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        var (loggerFactory, loggerVerifier) = GetMockedLoggerFactory<OptimisticConcurrencyException>();

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);

            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactory);
        });

        var entityId = Id.NewId();

        var firstTransactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        var secondTransactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        var firstTransaction = firstTransactionBuilder
            .Append(CommandSeeder.Create())
            .Build(Id.NewId());

        var secondTransaction = secondTransactionBuilder
            .Append(CommandSeeder.Create())
            .Build(Id.NewId());

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var firstTransactionInserted =
            await transactionRepository.PutTransaction(firstTransaction);
        var secondTransactionInserted =
            await transactionRepository.PutTransaction(secondTransaction);

        // ASSERT

        firstTransaction.Steps.Length.ShouldBe(1);
        secondTransaction.Steps.Length.ShouldBe(1);

        firstTransaction.Steps.ShouldAllBe(step => step.EntityId == entityId);
        secondTransaction.Steps.ShouldAllBe(step => step.EntityId == entityId);

        var firstCommandTransactionStep = firstTransaction.Steps[0]
            .ShouldBeAssignableTo<IAppendCommandTransactionStep>().ShouldNotBeNull();
        var secondCommandTransactionStep = secondTransaction.Steps[0]
            .ShouldBeAssignableTo<IAppendCommandTransactionStep>().ShouldNotBeNull();

        firstCommandTransactionStep.EntityVersionNumber.ShouldBe(secondCommandTransactionStep.EntityVersionNumber);

        firstTransactionInserted.ShouldBeTrue();
        secondTransactionInserted.ShouldBeFalse();

        loggerVerifier.Invoke(Times.Once());
    }

    private async Task Generic_GivenNonUniqueTags_WhenInsertingTagDocuments_ThenReturnTrue<TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var tag = TagSeeder.Create();

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var transaction = transactionBuilder
            .Add(tag)
            .Add(tag)
            .Build(default);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        // ASSERT

        transactionInserted.ShouldBeTrue();
    }

    private async Task Generic_GivenNonUniqueLeases_WhenInsertingLeaseDocuments_ThenReturnFalse<TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var lease = LeaseSeeder.Create();

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var transaction = transactionBuilder
            .Add(lease)
            .Add(lease)
            .Build(default);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        // ASSERT

        transactionInserted.ShouldBeFalse();
    }

    private async Task
        Generic_GivenCommandInserted_WhenGettingAnnotatedAgentSignature_ThenReturnAnnotatedAgentSignature<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        const ulong expectedCount = 5;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedTransactionId = Id.NewId();
        var expectedEntityId = Id.NewId();
        var expectedTransactionTimeStamp = transactionsAdder.FixTimeStamp(TimeStamp.UtcNow);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        var transaction = await BuildTransaction<TEntity>(serviceScope, expectedTransactionId, expectedEntityId,
            new[] { expectedCount }, expectedTransactionTimeStamp, agentSignature);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        var agentSignatureQuery = new EntityIdQuery(expectedEntityId);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var annotatedAgentSignatures = await transactionRepository.EnumerateAnnotatedAgentSignatures(agentSignatureQuery).ToArrayAsync();

        // ASSERT

        annotatedAgentSignatures.Length.ShouldBe(1);

        annotatedAgentSignatures[0].TransactionId.ShouldBe(expectedTransactionId);
        annotatedAgentSignatures[0].TransactionTimeStamp.ShouldBe(expectedTransactionTimeStamp);
        annotatedAgentSignatures[0].EntityIds.Length.ShouldBe(1);
        annotatedAgentSignatures[0].EntityIds[0].ShouldBe(expectedEntityId);
        annotatedAgentSignatures[0].Data.ShouldBeEquivalentTo(agentSignature);
    }

    private async Task Generic_GivenCommandInserted_WhenGettingAnnotatedCommand_ThenReturnAnnotatedCommand<TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        const ulong expectedCount = 5;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedTransactionId = Id.NewId();
        var expectedEntityId = Id.NewId();
        var expectedTransactionTimeStamp = transactionsAdder.FixTimeStamp(TimeStamp.UtcNow);

        var transaction = await BuildTransaction<TEntity>(serviceScope, expectedTransactionId, expectedEntityId,
            new[] { expectedCount }, expectedTransactionTimeStamp);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        var commandQuery = new GetEntityCommandsQuery(expectedEntityId, default);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var annotatedCommands = await transactionRepository.EnumerateAnnotatedCommands(commandQuery).ToArrayAsync();

        // ASSERT

        annotatedCommands.Length.ShouldBe(1);

        annotatedCommands[0].TransactionId.ShouldBe(expectedTransactionId);
        annotatedCommands[0].TransactionTimeStamp.ShouldBe(expectedTransactionTimeStamp);
        annotatedCommands[0].EntityId.ShouldBe(expectedEntityId);
        annotatedCommands[0].EntityVersionNumber.ShouldBe(new VersionNumber(1));

        var actualCountCommand = annotatedCommands[0].Data.ShouldBeAssignableTo<StoreNumber>().ShouldNotBeNull();

        actualCountCommand.Number.ShouldBe(expectedCount);
    }

    private async Task Generic_GivenEntityInserted_WhenGettingEntity_ThenReturnEntity<TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : class, IEntity<TEntity>, ISnapshotWithTestLogic<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        var expectedEntity = TEntity.Construct(entityId).WithVersionNumber(new VersionNumber(1));

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        var entityRepository = EntityRepository<TEntity>.Create(serviceScope.ServiceProvider, transactionRepository);

        var transaction = await BuildTransaction<TEntity>(serviceScope, Id.NewId(), entityId,
            new[] { 0UL });

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var actualEntity = await entityRepository.GetSnapshot(entityId);

        // ASSERT

        actualEntity.ShouldBeEquivalentTo(expectedEntity);
    }

    private async Task Generic_GivenEntityInsertedWithTags_WhenRemovingAllTags_ThenFinalEntityHasNoTags<TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        var tag = new Tag("Foo", "Bar");

        var expectedInitialTags = new[] { tag }.ToImmutableArray<ITag>();

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var initialTransaction = transactionBuilder
            .Add(tag)
            .Build(Id.NewId());

        var initialTransactionInserted = await transactionRepository.PutTransaction(initialTransaction);

        var tagQuery = new DeleteTagsQuery(entityId, expectedInitialTags);

        // ARRANGE ASSERTIONS

        initialTransactionInserted.ShouldBeTrue();

        // ACT

        var actualInitialTags = await transactionRepository.EnumerateTags(tagQuery).ToArrayAsync();

        var finalTransaction = transactionBuilder
            .Delete(tag)
            .Build(Id.NewId());

        var finalTransactionInserted = await transactionRepository.PutTransaction(finalTransaction);

        var actualFinalTags = await transactionRepository.EnumerateTags(tagQuery).ToArrayAsync();

        // ASSERT

        finalTransactionInserted.ShouldBeTrue();

        expectedInitialTags.SequenceEqual(actualInitialTags).ShouldBeTrue();

        actualFinalTags.ShouldBeEmpty();
    }

    private async Task Generic_GivenEntityInsertedWithLeases_WhenRemovingAllLeases_ThenFinalEntityHasNoLeases<TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var entityId = Id.NewId();

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, entityId);

        var lease = new Lease("Foo", "Bar", "Baz");

        var expectedInitialLeases = new[] { lease }.ToImmutableArray<ILease>();

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var initialTransaction = transactionBuilder
            .Add(lease)
            .Build(Id.NewId());

        var initialTransactionInserted = await transactionRepository.PutTransaction(initialTransaction);

        var leaseQuery = new DeleteLeasesQuery(entityId, expectedInitialLeases);

        // ARRANGE ASSERTIONS

        initialTransactionInserted.ShouldBeTrue();

        // ACT

        var actualInitialLeases = await transactionRepository.EnumerateLeases(leaseQuery).ToArrayAsync();

        var finalTransaction = transactionBuilder
            .Delete(lease)
            .Build(Id.NewId());

        var finalTransactionInserted = await transactionRepository.PutTransaction(finalTransaction);

        var actualFinalLeases = await transactionRepository.EnumerateLeases(leaseQuery).ToArrayAsync();

        // ASSERT

        finalTransactionInserted.ShouldBeTrue();

        actualInitialLeases.SequenceEqual(expectedInitialLeases).ShouldBeTrue();

        actualFinalLeases.ShouldBeEmpty();
    }

    private async Task
        Generic_GivenTransactionCreatesEntity_WhenQueryingForVersionOne_ThenReturnTheExpectedCommand<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedCommand = new StoreNumber(1);

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var transaction = transactionBuilder
            .Append(expectedCommand)
            .Build(Id.NewId());

        var versionOneCommandQuery = new EntityVersionNumberQuery(new VersionNumber(1), new VersionNumber(1));

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        var newCommands = await transactionRepository.EnumerateCommands(versionOneCommandQuery).ToArrayAsync();

        // ASSERT

        transactionInserted.ShouldBeTrue();

        transaction.Steps.Length.ShouldBe(1);

        var commandTransactionStep =
            transaction.Steps[0].ShouldBeAssignableTo<IAppendCommandTransactionStep>().ShouldNotBeNull();

        commandTransactionStep.EntityVersionNumber.ShouldBe(new VersionNumber(1));

        newCommands.Length.ShouldBe(1);

        newCommands[0].ShouldBeEquivalentTo(expectedCommand);
    }

    private async Task
        Generic_GivenTransactionAppendsEntityWithOneVersion_WhenQueryingForVersionTwo_ThenReturnExpectedCommand<
            TEntity>(TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var expectedCommand = new StoreNumber(2);

        var transactionBuilder = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionBuilderFactory<TEntity>>()
            .CreateForSingleEntity(default!, default);

        var firstTransaction = transactionBuilder
            .Append(new StoreNumber(1))
            .Build(Id.NewId());

        var secondTransaction = transactionBuilder
            .Append(expectedCommand)
            .Build(Id.NewId());

        var versionTwoCommandQuery = new EntityVersionNumberQuery(new VersionNumber(2), new VersionNumber(2));

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        var firstTransactionInserted = await transactionRepository.PutTransaction(firstTransaction);

        // ARRANGE ASSERTIONS

        firstTransactionInserted.ShouldBeTrue();

        // ACT

        var secondTransactionInserted = await transactionRepository.PutTransaction(secondTransaction);

        var newCommands = await transactionRepository.EnumerateCommands(versionTwoCommandQuery).ToArrayAsync();

        // ASSERT

        secondTransactionInserted.ShouldBeTrue();

        secondTransaction.Steps.Length.ShouldBe(1);

        var secondCommandTransactionStep = secondTransaction.Steps[0]
            .ShouldBeAssignableTo<IAppendCommandTransactionStep>().ShouldNotBeNull();

        secondCommandTransactionStep.EntityVersionNumber.ShouldBe(new VersionNumber(2));

        newCommands.Length.ShouldBe(1);

        newCommands[0].ShouldBeEquivalentTo(expectedCommand);
    }

    private async Task
        Generic_GivenTransactionAlreadyInserted_WhenQueryingByTransactionTimeStamp_ThenReturnExpectedObjects<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        const ulong timeSpanInMinutes = 60UL;
        const ulong gteInMinutes = 20UL;
        const ulong lteInMinutes = 30UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var originTimeStamp = TimeStamp.UnixEpoch;

        var transactions = new List<ITransaction>();
        var expectedObjects = new ExpectedObjects();

        var transactionIds = GetSortedIds((int)timeSpanInMinutes);
        var entityIds = GetSortedIds((int)timeSpanInMinutes);

        TimeStamp? gte = null;
        TimeStamp? lte = null;

        for (var i = 1UL; i <= timeSpanInMinutes; i++)
        {
            var currentTransactionId = transactionIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var currentTimeStamp = new TimeStamp(originTimeStamp.Value.AddMinutes(i));

            var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

            var commands = new object[] { new StoreNumber(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(gteInMinutes <= i && i <= lteInMinutes, currentTransactionId, currentEntityId,
                agentSignature, commands, leases, tags);

            switch (i)
            {
                case lteInMinutes:
                    lte = currentTimeStamp;
                    break;

                case gteInMinutes:
                    gte = currentTimeStamp;
                    break;
            }

            var transaction = await BuildTransaction<TEntity>(serviceScope, currentTransactionId, currentEntityId,
                new[] { i },
                currentTimeStamp, agentSignature);

            transactions.Add(transaction);
        }

        gte.ShouldNotBeNull();
        lte.ShouldNotBeNull();

        var query = new TransactionTimeStampQuery(gte.Value, lte.Value);

        await InsertTransactions(serviceScope, transactions);
        await TestGetTransactionIds(serviceScope, query as IAgentSignatureQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ICommandQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IAgentSignatureQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ICommandQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetCommands(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    private async Task
        Generic_GivenTransactionAlreadyInserted_WhenQueryingByTransactionId_ThenReturnExpectedObjects<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        const ulong numberOfTransactionIds = 10UL;
        const ulong whichTransactionId = 5UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var transactions = new List<ITransaction>();
        var expectedObjects = new ExpectedObjects();

        Id? transactionId = null;

        var transactionIds = GetSortedIds((int)numberOfTransactionIds);
        var entityIds = GetSortedIds((int)numberOfTransactionIds);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        for (var i = 1UL; i <= numberOfTransactionIds; i++)
        {
            var currentTransactionId = transactionIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var commands = new object[] { new StoreNumber(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i == whichTransactionId, currentTransactionId, currentEntityId, agentSignature,
                commands,
                leases, tags);

            if (i == whichTransactionId) transactionId = currentTransactionId;

            var transaction = await BuildTransaction<TEntity>(serviceScope, currentTransactionId, currentEntityId,
                new[] { i },
                agentSignatureOverride: agentSignature);

            transactions.Add(transaction);
        }

        transactionId.ShouldNotBeNull();

        var query = new TransactionIdQuery(transactionId.Value);

        await InsertTransactions(serviceScope, transactions);
        await TestGetTransactionIds(serviceScope, query as IAgentSignatureQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ICommandQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IAgentSignatureQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ICommandQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetCommands(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    private async Task
        Generic_GivenTransactionAlreadyInserted_WhenQueryingByEntityId_ThenReturnExpectedObjects<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        const ulong numberOfEntityIds = 10UL;
        const ulong whichEntityId = 5UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var transactions = new List<ITransaction>();
        var expectedObjects = new ExpectedObjects();

        Id? entityId = null;

        var transactionIds = GetSortedIds((int)numberOfEntityIds);
        var entityIds = GetSortedIds((int)numberOfEntityIds);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        for (var i = 1UL; i <= numberOfEntityIds; i++)
        {
            var currentTransactionId = transactionIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var commands = new object[] { new StoreNumber(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i == whichEntityId, currentTransactionId, currentEntityId, agentSignature, commands,
                leases, tags);

            if (i == whichEntityId) entityId = currentEntityId;

            var transaction = await BuildTransaction<TEntity>(serviceScope, currentTransactionId, currentEntityId,
                new[] { i },
                agentSignatureOverride: agentSignature);

            transactions.Add(transaction);
        }

        entityId.ShouldNotBeNull();

        var query = new EntityIdQuery(entityId.Value);

        await InsertTransactions(serviceScope, transactions);
        await TestGetTransactionIds(serviceScope, query as IAgentSignatureQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ICommandQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as IAgentSignatureQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ICommandQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetAgentSignatures(serviceScope, query, expectedObjects);
        await TestGetCommands(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    private async Task
        Generic_GivenTransactionAlreadyInserted_WhenQueryingByEntityVersionNumber_ThenReturnExpectedObjects<TEntity>(
            TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TEntity : IEntity<TEntity>
    {
        const ulong numberOfVersionNumbers = 20;
        const ulong gte = 5UL;
        const ulong lte = 15UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var counts = new List<ulong>();
        var expectedObjects = new ExpectedObjects();

        for (var i = 1UL; i <= numberOfVersionNumbers; i++)
        {
            var command = new StoreNumber(i);

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            counts.Add(i);

            expectedObjects.Add(i is >= gte and <= lte, default, default, default!, new[] { command },
                leases, tags);
        }

        var transaction = await BuildTransaction<TEntity>(serviceScope, Id.NewId(), Id.NewId(), counts.ToArray());

        var transactions = new List<ITransaction> { transaction };

        var query = new EntityVersionNumberQuery(new VersionNumber(gte), new VersionNumber(lte));

        await InsertTransactions(serviceScope, transactions);
        await TestGetCommands(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    private async Task Generic_GivenTransactionAlreadyInserted_WhenQueryingByData_ThenReturnExpectedObjects<TOptions, TEntity>(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
        where TOptions : class
        where TEntity : IEntity<TEntity>
    {
        const ulong countTo = 20UL;
        const ulong gte = 5UL;
        const ulong lte = 15UL;

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            transactionsAdder.AddDependencies.Invoke(serviceCollection);
            entityAdder.AddDependencies.Invoke(serviceCollection);
        });

        var transactions = new List<ITransaction>();
        var expectedObjects = new ExpectedObjects();

        var transactionIds = GetSortedIds((int)countTo);
        var entityIds = GetSortedIds((int)countTo);

        var agentSignature = new UnknownAgentSignature(new Dictionary<string, string>());

        var commands = new object[] { new DoNothing() };

        for (var i = 1UL; i <= countTo; i++)
        {
            var currentTransactionId = transactionIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i is >= gte and <= lte, currentTransactionId, currentEntityId, agentSignature, commands,
                leases, tags);

            var transaction = await BuildTransaction<TEntity>(serviceScope, currentTransactionId, currentEntityId,
                new[] { i },
                agentSignatureOverride: agentSignature);

            transactions.Add(transaction);
        }

        var options = serviceScope.ServiceProvider
            .GetRequiredService<IOptionsFactory<TOptions>>()
            .Create("Count");

        var query = new CountQuery(gte, lte, options);

        await InsertTransactions(serviceScope, transactions);
        await TestGetTransactionIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetTransactionIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ILeaseQuery, expectedObjects);
        await TestGetEntityIds(serviceScope, query as ITagQuery, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }


    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenReadOnlyMode_WhenPuttingTransaction_ThenCannotWriteInReadOnlyModeExceptionIsLogged(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenNonUniqueTransactionIds_WhenPuttingTransactions_ThenSecondPutReturnsFalse(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenNonUniqueVersionNumbers_WhenInsertingCommands_ThenReturnFalse(TransactionsAdder transactionsAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenVersionNumberZero_WhenInsertingCommands_ThenVersionZeroReservedExceptionIsLogged(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenNonUniqueVersionNumbers_WhenInsertingCommands_ThenOptimisticConcurrencyExceptionIsLogged(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenNonUniqueTags_WhenInsertingTagDocuments_ThenReturnTrue(TransactionsAdder transactionsAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenNonUniqueLeases_WhenInsertingLeaseDocuments_ThenReturnFalse(TransactionsAdder transactionsAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenCommandInserted_WhenGettingAnnotatedAgentSignature_ThenReturnAnnotatedAgentSignature(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenCommandInserted_WhenGettingAnnotatedCommand_ThenReturnAnnotatedCommand(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenEntityInserted_WhenGettingEntity_ThenReturnEntity(TransactionsAdder transactionsAdder,
        EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenEntityInsertedWithTags_WhenRemovingAllTags_ThenFinalEntityHasNoTags(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenEntityInsertedWithLeases_WhenRemovingAllLeases_ThenFinalEntityHasNoLeases(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenTransactionCreatesEntity_WhenQueryingForVersionOne_ThenReturnTheExpectedCommand(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenTransactionAppendsEntityWithOneVersion_WhenQueryingForVersionTwo_ThenReturnExpectedCommand(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenTransactionAlreadyInserted_WhenQueryingByTransactionTimeStamp_ThenReturnExpectedObjects(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenTransactionAlreadyInserted_WhenQueryingByTransactionId_ThenReturnExpectedObjects(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenTransactionAlreadyInserted_WhenQueryingByEntityId_ThenReturnExpectedObjects(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenTransactionAlreadyInserted_WhenQueryingByEntityVersionNumber_ThenReturnExpectedObjects(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    [Theory]
    [MemberData(nameof(AddTransactionsAndEntity))]
    public Task GivenTransactionAlreadyInserted_WhenQueryingByData_ThenReturnExpectedObjects(
        TransactionsAdder transactionsAdder, EntityAdder entityAdder)
    {
        return RunGenericTestAsync
        (
            new[] { transactionsAdder.QueryOptionsType, entityAdder.EntityType },
            new object?[] { transactionsAdder, entityAdder }
        );
    }

    private class ExpectedObjects
    {
        public readonly List<object> FalseAgentSignatures = new();
        public readonly List<object> FalseCommands = new();
        public readonly List<Id> FalseEntityIds = new();
        public readonly List<ILease> FalseLeases = new();
        public readonly List<ITag> FalseTags = new();
        public readonly List<Id> FalseTransactionIds = new();
        public readonly List<object> TrueAgentSignatures = new();

        public readonly List<object> TrueCommands = new();
        public readonly List<Id> TrueEntityIds = new();
        public readonly List<ILease> TrueLeases = new();
        public readonly List<ITag> TrueTags = new();
        public readonly List<Id> TrueTransactionIds = new();

        public void Add
        (
            bool condition,
            Id transactionId,
            Id entityId,
            object agentSignature,
            IEnumerable<object> commands,
            IEnumerable<ILease> leases,
            IEnumerable<ITag> tags
        )
        {
            if (condition)
            {
                TrueTransactionIds.Add(transactionId);
                TrueEntityIds.Add(entityId);
                TrueAgentSignatures.Add(agentSignature);
                TrueCommands.AddRange(commands);
                TrueLeases.AddRange(leases);
                TrueTags.AddRange(tags);
            }
            else
            {
                FalseTransactionIds.Add(transactionId);
                FalseEntityIds.Add(entityId);
                FalseAgentSignatures.Add(agentSignature);
                FalseCommands.AddRange(commands);
                FalseLeases.AddRange(leases);
                FalseTags.AddRange(tags);
            }
        }
    }
}
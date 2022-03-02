using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Leases;
using EntityDb.Common.Queries;
using EntityDb.Common.Queries.Modified;
using EntityDb.Common.Tags;
using EntityDb.Common.Tests.Implementations.Agents;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Queries;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Tags;
using EntityDb.Common.Transactions;
using EntityDb.Common.Transactions.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace EntityDb.Common.Tests.Transactions;

public abstract class TransactionTestsBase<TStartup> : TestsBase<TStartup>
    where TStartup : IStartup, new()
{
    protected TransactionTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
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

    private static ModifiedQueryOptions NewModifiedQueryOptions(bool invertFilter, bool reverseSort, int? replaceSkip, int? replaceTake)
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
        Func<ITransactionRepository, ModifiedQueryOptions, Task<TResult[]>> getActualResults,
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
        var expectedSkipTakeResults = expectedTrueResults.Skip(1).Take(1);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(secondaryPreferred ? TestSessionOptions.ReadOnlySecondaryPreferred : TestSessionOptions.ReadOnly);

        // ACT

        var actualTrueResults =
            await getActualResults.Invoke(transactionRepository, bufferModifier);
        var actualFalseResults =
            await getActualResults.Invoke(transactionRepository, negateModifier);
        var reversedActualTrueResults =
            await getActualResults.Invoke(transactionRepository, reverseBufferModifier);
        var reversedActualFalseResults =
            await getActualResults.Invoke(transactionRepository, reverseNegateModifier);
        var actualSkipTakeResults =
            await getActualResults.Invoke(transactionRepository, bufferSubsetModifier);

        // ASSERT

        actualTrueResults.SequenceEqual(expectedTrueResults).ShouldBeTrue();
        actualFalseResults.SequenceEqual(expectedFalseResults).ShouldBeTrue();
        reversedActualTrueResults.SequenceEqual(reversedExpectedTrueResults).ShouldBeTrue();
        reversedActualFalseResults.SequenceEqual(reversedExpectedFalseResults).ShouldBeTrue();
        actualSkipTakeResults.SequenceEqual(expectedSkipTakeResults).ShouldBeTrue();
    }

    private static async Task TestGetTransactionIds
    (
        IServiceScope serviceScope,
        IAgentSignatureQuery query,
        ExpectedObjects expectedObjects
    )
    {
        Guid[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTransactionIds
                    : expectedObjects.TrueTransactionIds)
                .ToArray();
        }

        Task<Guid[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetTransactionIds(query.Modify(modifiedQueryOptions));
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
        Guid[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTransactionIds
                    : expectedObjects.TrueTransactionIds)
                .ToArray();
        }

        Task<Guid[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetTransactionIds(query.Modify(modifiedQueryOptions));
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
        Guid[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTransactionIds
                    : expectedObjects.TrueTransactionIds)
                .ToArray();
        }

        Task<Guid[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetTransactionIds(query.Modify(modifiedQueryOptions));
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
        Guid[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseTransactionIds
                    : expectedObjects.TrueTransactionIds)
                .ToArray();
        }

        Task<Guid[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetTransactionIds(query.Modify(modifiedQueryOptions));
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
        Guid[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        Task<Guid[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetEntityIds(query.Modify(modifiedQueryOptions));
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
        Guid[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        Task<Guid[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetEntityIds(query.Modify(modifiedQueryOptions));
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
        Guid[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        Task<Guid[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetEntityIds(query.Modify(modifiedQueryOptions));
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
        Guid[] GetExpectedResults(bool invert)
        {
            return (invert
                    ? expectedObjects.FalseEntityIds
                    : expectedObjects.TrueEntityIds)
                .ToArray();
        }

        Task<Guid[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetEntityIds(query.Modify(modifiedQueryOptions));
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

        Task<object[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetAgentSignatures(query.Modify(modifiedQueryOptions));
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

        Task<object[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetCommands(query.Modify(modifiedQueryOptions));
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

        Task<ILease[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetLeases(query.Modify(modifiedQueryOptions));
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

        Task<ITag[]> GetActualResults(ITransactionRepository transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
        {
            return transactionRepository.GetTags(query.Modify(modifiedQueryOptions));
        }

        await TestGet(serviceScope, GetExpectedResults, GetActualResults, true);
        await TestGet(serviceScope, GetExpectedResults, GetActualResults, false);
    }

    private static ITransaction BuildTransaction
    (
        IServiceScope serviceScope,
        Guid transactionId,
        Guid entityId,
        IEnumerable<int> counts,
        DateTime? timeStampOverride = null,
        object? agentSignatureOverride = null
    )
    {
        var transactionBuilder = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(entityId);

        foreach (var count in counts)
        {
            transactionBuilder.Append(new Count(count));
            transactionBuilder.Add(new CountLease(count));
            transactionBuilder.Add(new CountTag(count));
        }

        var transaction = (transactionBuilder.Build(default!, transactionId) as Transaction)!;
            
        if (timeStampOverride.HasValue)
        {
            transaction = transaction with
            {
                TimeStamp = timeStampOverride.Value
            };
        }

        if (agentSignatureOverride != null)
        {
            transaction = transaction with
            {
                AgentSignature = agentSignatureOverride
            };
        }

        return transaction;
    }

    private static Guid[] GetSortedGuids(int numberOfGuids)
    {
        return Enumerable
            .Range(1, numberOfGuids)
            .Select(_ => Guid.NewGuid())
            .OrderBy(guid => guid)
            .ToArray();
    }

    [Fact]
    public async Task GivenReadOnlyMode_WhenPuttingTransaction_ThenCannotWriteInReadOnlyModeExceptionIsLogged()
    {
        // ARRANGE

        var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

        loggerMock
            .Setup(logger => logger.LogError(It.IsAny<CannotWriteInReadOnlyModeException>(), It.IsAny<string>()))
            .Verifiable();

        var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);

        loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<Type>()))
            .Returns(loggerMock.Object);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactoryMock.Object);
        });

        var transaction = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(default)
            .Append(CommandSeeder.Create())
            .Build(default!, default);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.ReadOnly);

        // ACT

        var inserted = await transactionRepository.PutTransaction(transaction);

        // ASSERT

        inserted.ShouldBeFalse();

        loggerMock.Verify();
    }

    [Fact]
    public async Task GivenNonUniqueTransactionIds_WhenPuttingTransactions_ThenSecondPutReturnsFalse()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var transactionId = Guid.NewGuid();
            
            
        var firstTransaction = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(default)
            .Append(CommandSeeder.Create())
            .Build(default!, transactionId);
            
        var secondTransaction = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(default)
            .Append(CommandSeeder.Create())
            .Build(default!, transactionId);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var firstTransactionInserted = await transactionRepository.PutTransaction(firstTransaction);
        var secondTransactionInserted = await transactionRepository.PutTransaction(secondTransaction);

        // ASSERT

        firstTransactionInserted.ShouldBeTrue();
        secondTransactionInserted.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenNonUniqueVersionNumbers_WhenInsertingCommands_ThenReturnFalse()
    {
        // ARRANGE

        const int repeatCount = 2;

        using var serviceScope = CreateServiceScope();

        var transaction = (serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .ForSingleEntity(default)
                .Append(CommandSeeder.Create())
                .Build(default!, default)
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

    [Fact]
    public async Task
        GivenVersionNumberZero_WhenInsertingCommands_ThenVersionZeroReservedExceptionIsLogged()
    {
        // ARRANGE

        const ulong versionNumber = 0UL;
            
        var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

        loggerMock
            .Setup(logger => logger.LogError(It.IsAny<VersionZeroReservedException>(), It.IsAny<string>()))
            .Verifiable();

        var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);

        loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<Type>()))
            .Returns(loggerMock.Object);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactoryMock.Object);
        });

        var transactionStepMock = new Mock<IAppendCommandTransactionStep>(MockBehavior.Strict);

        transactionStepMock
            .SetupGet(step => step.EntityId)
            .Returns(default(Guid));
            
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

        loggerMock.Verify();
    }

    [Fact]
    public async Task
        GivenNonUniqueVersionNumbers_WhenInsertingCommands_ThenOptimisticConcurrencyExceptionIsLogged()
    {
        // ARRANGE

        var loggerMock = new Mock<ILogger>(MockBehavior.Strict);

        loggerMock
            .Setup(logger => logger.LogError(It.IsAny<OptimisticConcurrencyException>(), It.IsAny<string>()))
            .Verifiable();

        var loggerFactoryMock = new Mock<ILoggerFactory>(MockBehavior.Strict);

        loggerFactoryMock
            .Setup(factory => factory.CreateLogger(It.IsAny<Type>()))
            .Returns(loggerMock.Object);

        using var serviceScope = CreateServiceScope(serviceCollection =>
        {
            serviceCollection.RemoveAll(typeof(ILoggerFactory));

            serviceCollection.AddSingleton(loggerFactoryMock.Object);
        });

        var entityId = Guid.NewGuid();

        var firstTransaction = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(entityId)
            .Append(CommandSeeder.Create())
            .Build(default!, Guid.NewGuid());
            
        var secondTransaction = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(entityId)
            .Append(CommandSeeder.Create())
            .Build(default!, Guid.NewGuid());
            
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
        
        var firstCommandTransactionStep = firstTransaction.Steps[0].ShouldBeAssignableTo<IAppendCommandTransactionStep>()!;
        var secondCommandTransactionStep = secondTransaction.Steps[0].ShouldBeAssignableTo<IAppendCommandTransactionStep>()!;

        firstCommandTransactionStep.EntityVersionNumber.ShouldBe(secondCommandTransactionStep.EntityVersionNumber);

        firstTransactionInserted.ShouldBeTrue();
        secondTransactionInserted.ShouldBeFalse();

        loggerMock.Verify();
    }

    [Fact]
    public async Task GivenNonUniqueTags_WhenInsertingTagDocuments_ThenReturnTrue()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var tag = TagSeeder.Create();
            
        var transaction = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(default)
            .Add(tag)
            .Add(tag)
            .Build(default!, default);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        // ASSERT

        transactionInserted.ShouldBeTrue();
    }

    [Fact]
    public async Task GivenNonUniqueLeases_WhenInsertingLeaseDocuments_ThenReturnFalse()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var lease = LeaseSeeder.Create();
            
        var transaction = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(default)
            .Add(lease)
            .Add(lease)
            .Build(default!, default);
            
        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        // ACT

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        // ASSERT

        transactionInserted.ShouldBeFalse();
    }

    [Fact]
    public async Task GivenCommandInserted_WhenGettingAnnotatedCommand_ThenReturnAnnotatedCommand()
    {
        // ARRANGE

        const int expectedCount = 5;
        
        using var serviceScope = CreateServiceScope();

        var transactionTimeStamp = DateTime.UtcNow;

        var expectedTransactionId = Guid.NewGuid();
        var expectedEntityId = Guid.NewGuid();
        var expectedTransactionTimeStamps = new[]
        {
            transactionTimeStamp,

            // A .NET DateTime can be more precise than milliseconds.
            // This allows for database types that cannot be more precise than milliseconds.
            transactionTimeStamp - TimeSpan.FromTicks(transactionTimeStamp.Ticks % TimeSpan.TicksPerMillisecond)
        };

        var transaction = BuildTransaction(serviceScope, expectedTransactionId, expectedEntityId,
            new[] { expectedCount }, transactionTimeStamp);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        var commandQuery = new GetCurrentEntityQuery(expectedEntityId, 0);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var annotatedCommands = await transactionRepository.GetAnnotatedCommands(commandQuery);

        // ASSERT

        annotatedCommands.Length.ShouldBe(1);

        annotatedCommands[0].TransactionId.ShouldBe(expectedTransactionId);
        annotatedCommands[0].EntityId.ShouldBe(expectedEntityId);
        annotatedCommands[0].EntityVersionNumber.ShouldBe(1ul);
            
        var actualCountCommand = annotatedCommands[0].Data.ShouldBeAssignableTo<Count>()!;

        actualCountCommand.Number.ShouldBe(expectedCount);
            
        expectedTransactionTimeStamps.Contains(annotatedCommands[0].TransactionTimeStamp).ShouldBeTrue();
    }

    [Fact]
    public async Task GivenEntityInserted_WhenGettingEntity_ThenReturnEntity()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var expectedEntity = new TransactionEntity { VersionNumber = 1 };

        var entityId = Guid.NewGuid();

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        var entityRepository = EntityRepository<TransactionEntity>.Create(serviceScope.ServiceProvider, transactionRepository);

        var transaction = BuildTransaction(serviceScope, Guid.NewGuid(), entityId,
            new[] { 0 });

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        // ARRANGE ASSERTIONS

        transactionInserted.ShouldBeTrue();

        // ACT

        var actualEntity = await entityRepository.GetCurrent(entityId);

        // ASSERT

        actualEntity.ShouldBeEquivalentTo(expectedEntity);
    }

    [Fact]
    public async Task GivenEntityInsertedWithTags_WhenRemovingAllTags_ThenFinalEntityHasNoTags()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var entityId = Guid.NewGuid();

        var transactionBuilder = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(entityId);

        var tag = new Tag("Foo", "Bar");

        var expectedInitialTags = new[] { tag }.ToImmutableArray<ITag>();

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var initialTransaction = transactionBuilder
            .Add(tag)
            .Build(default!, Guid.NewGuid());

        var initialTransactionInserted = await transactionRepository.PutTransaction(initialTransaction);

        var tagQuery = new DeleteTagsQuery(entityId, expectedInitialTags);

        // ARRANGE ASSERTIONS

        initialTransactionInserted.ShouldBeTrue();

        // ACT

        var actualInitialTags = await transactionRepository.GetTags(tagQuery);

        var finalTransaction = transactionBuilder
            .Delete(tag)
            .Build(default!, Guid.NewGuid());

        var finalTransactionInserted = await transactionRepository.PutTransaction(finalTransaction);

        var actualFinalTags = await transactionRepository.GetTags(tagQuery);

        // ASSERT

        finalTransactionInserted.ShouldBeTrue();

        expectedInitialTags.SequenceEqual(actualInitialTags).ShouldBeTrue();

        actualFinalTags.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenEntityInsertedWithLeases_WhenRemovingAllLeases_ThenFinalEntityHasNoLeases()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var entityId = Guid.NewGuid();

        var transactionBuilder = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(entityId);

        var lease = new Lease("Foo", "Bar", "Baz");

        var expectedInitialLeases = new[] { lease }.ToImmutableArray<ILease>();

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        var initialTransaction = transactionBuilder
            .Add(lease)
            .Build(default!, Guid.NewGuid());

        var initialTransactionInserted = await transactionRepository.PutTransaction(initialTransaction);

        var leaseQuery = new DeleteLeasesQuery(entityId, expectedInitialLeases);

        // ARRANGE ASSERTIONS

        initialTransactionInserted.ShouldBeTrue();

        // ACT

        var actualInitialLeases = await transactionRepository.GetLeases(leaseQuery);

        var finalTransaction = transactionBuilder
            .Delete(lease)
            .Build(default!, Guid.NewGuid());

        var finalTransactionInserted = await transactionRepository.PutTransaction(finalTransaction);

        var actualFinalLeases = await transactionRepository.GetLeases(leaseQuery);

        // ASSERT

        finalTransactionInserted.ShouldBeTrue();

        actualInitialLeases.SequenceEqual(expectedInitialLeases).ShouldBeTrue();

        actualFinalLeases.ShouldBeEmpty();
    }

    [Fact]
    public async Task GivenTransactionCreatesEntity_WhenQueryingForVersionOne_ThenReturnTheExpectedCommand()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var expectedCommand = new Count(1);

        var transactionBuilder = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(default);

        var transaction = transactionBuilder
            .Append(expectedCommand)
            .Build(default!, Guid.NewGuid());

        var versionOneCommandQuery = new EntityVersionNumberQuery(1, 1);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>()
            .CreateRepository(TestSessionOptions.Write);

        // ACT

        var transactionInserted = await transactionRepository.PutTransaction(transaction);

        var newCommands = await transactionRepository.GetCommands(versionOneCommandQuery);

        // ASSERT

        transactionInserted.ShouldBeTrue();

        transaction.Steps.Length.ShouldBe(1);

        var commandTransactionStep = transaction.Steps[0].ShouldBeAssignableTo<IAppendCommandTransactionStep>()!;

        commandTransactionStep.EntityVersionNumber.ShouldBe(1ul);

        newCommands.Length.ShouldBe(1);

        newCommands[0].ShouldBeEquivalentTo(expectedCommand);
    }

    [Fact]
    public async Task
        GivenTransactionAppendsEntityWithOneVersion_WhenQueryingForVersionTwo_ThenReturnExpectedCommand()
    {
        // ARRANGE

        using var serviceScope = CreateServiceScope();

        var expectedCommand = new Count(2);

        var transactionBuilder = serviceScope.ServiceProvider
            .GetRequiredService<TransactionBuilder<TransactionEntity>>()
            .ForSingleEntity(default);

        var firstTransaction = transactionBuilder
            .Append(new Count(1))
            .Build(default!, Guid.NewGuid());

        var secondTransaction = transactionBuilder
            .Append(expectedCommand)
            .Build(default!, Guid.NewGuid());

        var versionTwoCommandQuery = new EntityVersionNumberQuery(2, 2);

        await using var transactionRepository = await serviceScope.ServiceProvider
            .GetRequiredService<ITransactionRepositoryFactory>().CreateRepository(TestSessionOptions.Write);

        var firstTransactionInserted = await transactionRepository.PutTransaction(firstTransaction);

        // ARRANGE ASSERTIONS

        firstTransactionInserted.ShouldBeTrue();

        // ACT

        var secondTransactionInserted = await transactionRepository.PutTransaction(secondTransaction);

        var newCommands = await transactionRepository.GetCommands(versionTwoCommandQuery);

        // ASSERT

        secondTransactionInserted.ShouldBeTrue();

        secondTransaction.Steps.Length.ShouldBe(1);

        var secondCommandTransactionStep = secondTransaction.Steps[0].ShouldBeAssignableTo<IAppendCommandTransactionStep>()!;

        secondCommandTransactionStep.EntityVersionNumber.ShouldBe(2ul);

        newCommands.Length.ShouldBe(1);

        newCommands[0].ShouldBeEquivalentTo(expectedCommand);
    }

    [Theory]
    [InlineData(60, 20, 30)]
    public async Task GivenTransactionAlreadyInserted_WhenQueryingByTransactionTimeStamp_ThenReturnExpectedObjects(
        int timeSpanInMinutes, int gteInMinutes, int lteInMinutes)
    {
        using var serviceScope = CreateServiceScope();

        var originTimeStamp = DateTime.UnixEpoch;

        var transactions = new List<ITransaction>();
        var expectedObjects = new ExpectedObjects();

        var transactionIds = GetSortedGuids(timeSpanInMinutes);
        var entityIds = GetSortedGuids(timeSpanInMinutes);

        DateTime? gte = null;
        DateTime? lte = null;

        for (var i = 1; i <= timeSpanInMinutes; i++)
        {
            var currentTransactionId = transactionIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var currentTimeStamp = originTimeStamp.AddMinutes(i);

            var agentSignature = new CounterAgentSignature(i);

            var commands = new object[] { new Count(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(gteInMinutes <= i && i <= lteInMinutes, currentTransactionId, currentEntityId,
                agentSignature, commands, leases, tags);

            if (i == lteInMinutes)
            {
                lte = currentTimeStamp;
            }
            else if (i == gteInMinutes)
            {
                gte = currentTimeStamp;
            }

            var transaction = BuildTransaction(serviceScope, currentTransactionId, currentEntityId, new[]{i},
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

    [Theory]
    [InlineData(10, 5)]
    public async Task GivenTransactionAlreadyInserted_WhenQueryingByTransactionId_ThenReturnExpectedObjects(
        int numberOfTransactionIds, int whichTransactionId)
    {
        using var serviceScope = CreateServiceScope();

        var transactions = new List<ITransaction>();
        var expectedObjects = new ExpectedObjects();

        Guid? transactionId = null;

        var transactionIds = GetSortedGuids(numberOfTransactionIds);
        var entityIds = GetSortedGuids(numberOfTransactionIds);

        for (var i = 1; i <= numberOfTransactionIds; i++)
        {
            var currentTransactionId = transactionIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var agentSignature = new CounterAgentSignature(i);

            var commands = new object[] { new Count(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i == whichTransactionId, currentTransactionId, currentEntityId, agentSignature, commands,
                leases, tags);

            if (i == whichTransactionId)
            {
                transactionId = currentTransactionId;
            }

            var transaction = BuildTransaction(serviceScope, currentTransactionId, currentEntityId, new[]{i},
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

    [Theory]
    [InlineData(10, 5)]
    public async Task GivenTransactionAlreadyInserted_WhenQueryingByEntityId_ThenReturnExpectedObjects(
        int numberOfEntityIds, int whichEntityId)
    {
        using var serviceScope = CreateServiceScope();

        var transactions = new List<ITransaction>();
        var expectedObjects = new ExpectedObjects();

        Guid? entityId = null;

        var transactionIds = GetSortedGuids(numberOfEntityIds);
        var entityIds = GetSortedGuids(numberOfEntityIds);

        for (var i = 1; i <= numberOfEntityIds; i++)
        {
            var currentTransactionId = transactionIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var agentSignature = new CounterAgentSignature(i);

            var commands = new object[] { new Count(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(i == whichEntityId, currentTransactionId, currentEntityId, agentSignature, commands,
                leases, tags);

            if (i == whichEntityId)
            {
                entityId = currentEntityId;
            }

            var transaction = BuildTransaction(serviceScope, currentTransactionId, currentEntityId, new[]{i},
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

    [Theory]
    [InlineData(20, 5, 15)]
    public async Task GivenTransactionAlreadyInserted_WhenQueryingByEntityVersionNumber_ThenReturnExpectedObjects(
        int numberOfVersionNumbers, int gteAsInt, int lteAsInt)
    {
        using var serviceScope = CreateServiceScope();

        var counts = new List<int>();
        var expectedObjects = new ExpectedObjects();

        for (var i = 1; i <= numberOfVersionNumbers; i++)
        {
            var command = new Count(i);

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            counts.Add(i);

            expectedObjects.Add(gteAsInt <= i && i <= lteAsInt, default, default, default!, new[] { command },
                leases, tags);
        }

        var transaction = BuildTransaction(serviceScope, Guid.NewGuid(), Guid.NewGuid(), counts.ToArray());

        var transactions = new List<ITransaction> { transaction };

        var query = new EntityVersionNumberQuery((ulong)gteAsInt, (ulong)lteAsInt);

        await InsertTransactions(serviceScope, transactions);
        await TestGetCommands(serviceScope, query, expectedObjects);
        await TestGetLeases(serviceScope, query, expectedObjects);
        await TestGetTags(serviceScope, query, expectedObjects);
    }

    [Theory]
    [InlineData(20, 5, 15)]
    public async Task GivenTransactionAlreadyInserted_WhenQueryingByData_ThenReturnExpectedObjects(int countTo,
        int gte, int lte)
    {
        using var serviceScope = CreateServiceScope();

        var transactions = new List<ITransaction>();
        var expectedObjects = new ExpectedObjects();

        var transactionIds = GetSortedGuids(countTo);
        var entityIds = GetSortedGuids(countTo);

        for (var i = 1; i <= countTo; i++)
        {
            var currentTransactionId = transactionIds[i - 1];
            var currentEntityId = entityIds[i - 1];

            var agentSignature = new CounterAgentSignature(i);

            var commands = new object[] { new Count(i) };

            var leases = new[] { new CountLease(i) };

            var tags = new[] { new CountTag(i) };

            expectedObjects.Add(gte <= i && i <= lte, currentTransactionId, currentEntityId, agentSignature, commands,
                leases, tags);

            var transaction = BuildTransaction(serviceScope, currentTransactionId, currentEntityId, new[]{i},
                agentSignatureOverride: agentSignature);

            transactions.Add(transaction);
        }

        var query = new CountQuery(gte, lte);

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

    private class ExpectedObjects
    {
        public readonly List<object> FalseCommands = new();
        public readonly List<Guid> FalseEntityIds = new();
        public readonly List<ILease> FalseLeases = new();
        public readonly List<object> FalseAgentSignatures = new();
        public readonly List<ITag> FalseTags = new();
        public readonly List<Guid> FalseTransactionIds = new();

        public readonly List<object> TrueCommands = new();
        public readonly List<Guid> TrueEntityIds = new();
        public readonly List<ILease> TrueLeases = new();
        public readonly List<object> TrueAgentSignatures = new();
        public readonly List<ITag> TrueTags = new();
        public readonly List<Guid> TrueTransactionIds = new();

        public void Add
        (
            bool condition,
            Guid transactionId,
            Guid entityId,
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
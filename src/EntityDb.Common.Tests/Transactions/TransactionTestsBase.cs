using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Loggers;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Common.Entities;
using EntityDb.Common.Exceptions;
using EntityDb.Common.Extensions;
using EntityDb.Common.Leases;
using EntityDb.Common.Queries;
using EntityDb.Common.Queries.Modified;
using EntityDb.Common.Tags;
using EntityDb.Common.Tests.Implementations.AgentSignature;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Entities;
using EntityDb.Common.Tests.Implementations.Leases;
using EntityDb.Common.Tests.Implementations.Queries;
using EntityDb.Common.Tests.Implementations.Seeders;
using EntityDb.Common.Tests.Implementations.Tags;
using EntityDb.Common.Transactions;
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

namespace EntityDb.Common.Tests.Transactions
{
    public abstract class TransactionTestsBase<TStartup> : TestsBase<TStartup>
        where TStartup : IStartup, new()
    {
        protected TransactionTestsBase(IServiceProvider startupServiceProvider) : base(startupServiceProvider)
        {
        }

        private async Task InsertTransactions
        (
            IServiceScope serviceScope,
            List<ITransaction<TransactionEntity>> transactions
        )
        {
            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>().CreateRepository(TestSessionOptions.Write);

            foreach (var transaction in transactions)
            {
                var transactionInserted = await transactionRepository.PutTransaction(transaction);

                transactionInserted.ShouldBeTrue();
            }
        }

        private ModifiedQueryOptions NewModifiedQueryOptions(bool invertFilter, bool reverseSort, int? replaceSkip, int? replaceTake)
        {
            return new ModifiedQueryOptions
            {
                InvertFilter = invertFilter,
                ReverseSort = reverseSort,
                ReplaceSkip = replaceSkip,
                ReplaceTake = replaceTake,
            };
        }

        private async Task TestGet<TResult>
        (
            IServiceScope serviceScope,
            Func<bool, TResult[]> getExpectedResults,
            Func<ITransactionRepository<TransactionEntity>, ModifiedQueryOptions, Task<TResult[]>> getActualResults,
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
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>()
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

        private async Task TestGetTransactionIds
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

            Task<Guid[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetTransactionIds(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetTransactionIds
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

            Task<Guid[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetTransactionIds(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetTransactionIds
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

            Task<Guid[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetTransactionIds(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetTransactionIds
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

            Task<Guid[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetTransactionIds(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetEntityIds
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

            Task<Guid[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetEntityIds(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetEntityIds
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

            Task<Guid[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetEntityIds(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetEntityIds
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

            Task<Guid[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetEntityIds(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetEntityIds
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

            Task<Guid[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetEntityIds(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetAgentSignatures
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

            Task<object[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetAgentSignatures(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetCommands
        (
            IServiceScope serviceScope,
            ICommandQuery query,
            ExpectedObjects expectedObjects
        )
        {
            ICommand<TransactionEntity>[] GetExpectedResults(bool invert)
            {
                return (invert
                    ? expectedObjects.FalseCommands
                    : expectedObjects.TrueCommands)
                    .ToArray();
            }

            Task<ICommand<TransactionEntity>[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetCommands(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetLeases
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

            Task<ILease[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetLeases(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private async Task TestGetTags
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

            Task<ITag[]> GetActualResults(ITransactionRepository<TransactionEntity> transactionRepository, ModifiedQueryOptions modifiedQueryOptions)
            {
                return transactionRepository.GetTags(query.Modify(modifiedQueryOptions));
            }

            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: true);
            await TestGet(serviceScope, GetExpectedResults, GetActualResults, secondaryPreferred: false);
        }

        private ITransaction<TransactionEntity> BuildTransaction
        (
            IServiceScope serviceScope,
            Guid transactionId,
            Guid entityId,
            ICommand<TransactionEntity>[] commands,
            DateTime? timeStampOverride = null,
            object? agentSignatureOverride = null
        )
        {
            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            transactionBuilder.Create(entityId, commands[0]);

            for (var i = 1; i < commands.Length; i++)
            {
                transactionBuilder.Append(entityId, commands[i]);
            }

            return transactionBuilder.Build(transactionId, timeStampOverride, agentSignatureOverride);
        }

        private Guid[] GetSortedGuids(int numberOfGuids)
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

            var transaction = TransactionSeeder.Create(1, 1);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>()
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

            var firstTransaction = TransactionSeeder.Create(1, 1, transactionId);
            var secondTransaction = TransactionSeeder.Create(1, 1, transactionId);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>().CreateRepository(TestSessionOptions.Write);

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

            using var serviceScope = CreateServiceScope();

            var transaction = TransactionSeeder.Create(1, 2);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>().CreateRepository(TestSessionOptions.Write);

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

            var transaction = TransactionSeeder.Create(1, 1, wellBehavedNextEntityVersionNumber: false);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>()
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

            var firstTransaction = TransactionSeeder.Create(1, 1, entityId: entityId);
            var secondTransaction = TransactionSeeder.Create(1, 1, entityId: entityId);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>()
                .CreateRepository(TestSessionOptions.Write);


            // ACT

            var firstTransactionInserted =
                await transactionRepository.PutTransaction(firstTransaction);
            var secondTransactionInserted =
                await transactionRepository.PutTransaction(secondTransaction);

            // ASSERT

            firstTransaction.Steps.Length.ShouldBe(1);
            secondTransaction.Steps.Length.ShouldBe(1);

            firstTransaction.Steps[0].EntityId.ShouldBe(secondTransaction.Steps[0].EntityId);
            firstTransaction.Steps[0].NextEntityVersionNumber
                .ShouldBe(secondTransaction.Steps[0].NextEntityVersionNumber);

            firstTransactionInserted.ShouldBeTrue();
            secondTransactionInserted.ShouldBeFalse();

            loggerMock.Verify();
        }

        [Fact]
        public async Task GivenNonUniqueTags_WhenInsertingTagDocuments_ThenReturnTrue()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var transaction = TransactionSeeder.Create(2, 1, insertTag: true);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>().CreateRepository(TestSessionOptions.Write);

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

            var transaction = TransactionSeeder.Create(2, 1, insertLease: true);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>().CreateRepository(TestSessionOptions.Write);

            // ACT

            var transactionInserted = await transactionRepository.PutTransaction(transaction);

            // ASSERT

            transactionInserted.ShouldBeFalse();
        }

        [Fact]
        public async Task GivenCommandInserted_WhenGettingAnnotatedCommand_ThenReturnAnnotatedCommand()
        {
            // ARRANGE

            using var serviceScope = CreateServiceScope();

            var transactionTimeStamp = DateTime.UtcNow;

            var expectedTransactionId = Guid.NewGuid();
            var expectedEntityId = Guid.NewGuid();
            var expectedCommand = new Count(5);
            var expectedTransactionTimeStamps = new[]
            {
                transactionTimeStamp,

                // A .NET DateTime can be more precise than milliseconds.
                // This allows for database types that cannot be more precise than milliseconds.
                transactionTimeStamp - TimeSpan.FromTicks(transactionTimeStamp.Ticks % TimeSpan.TicksPerMillisecond)
            };

            var transaction = BuildTransaction(serviceScope, expectedTransactionId, expectedEntityId,
                new ICommand<TransactionEntity>[] { expectedCommand }, transactionTimeStamp);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>()
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
            annotatedCommands[0].Data.ShouldBe(expectedCommand);

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
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>().CreateRepository(TestSessionOptions.Write);

            var entityRepository = EntityRepository<TransactionEntity>.Create(serviceScope.ServiceProvider, transactionRepository);

            var transaction = BuildTransaction(serviceScope, Guid.NewGuid(), entityId,
                new ICommand<TransactionEntity>[] { new DoNothing() });

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

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            var expectedInitialTags = new[] { new Tag("Foo", "Bar") }.ToImmutableArray<ITag>();

            var entityId = Guid.NewGuid();

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>()
                .CreateRepository(TestSessionOptions.Write);

            var initialTransaction = transactionBuilder
                .Create(entityId, new AddTag("Foo", "Bar"))
                .Build(Guid.NewGuid());

            var initialTransactionInserted = await transactionRepository.PutTransaction(initialTransaction);

            var tagQuery = new DeleteTagsQuery(entityId, expectedInitialTags);

            // ARRANGE ASSERTIONS

            initialTransactionInserted.ShouldBeTrue();

            // ACT

            var actualInitialTags = await transactionRepository.GetTags(tagQuery);

            var finalTransaction = transactionBuilder
                .Append(entityId, new RemoveAllTags())
                .Build(Guid.NewGuid());

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

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            var expectedInitialLeases = new[] { new Lease("Foo", "Bar", "Baz") }.ToImmutableArray<ILease>();

            var entityId = Guid.NewGuid();

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>()
                .CreateRepository(TestSessionOptions.Write);

            var initialTransaction = transactionBuilder
                .Create(entityId, new AddLease("Foo", "Bar", "Baz"))
                .Build(Guid.NewGuid());

            var initialTransactionInserted = await transactionRepository.PutTransaction(initialTransaction);

            var leaseQuery = new DeleteLeasesQuery(entityId, expectedInitialLeases);

            // ARRANGE ASSERTIONS

            initialTransactionInserted.ShouldBeTrue();

            // ACT

            var actualInitialLeases = await transactionRepository.GetLeases(leaseQuery);

            var finalTransaction = transactionBuilder
                .Append(entityId, new RemoveAllLeases())
                .Build(Guid.NewGuid());

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

            var transaction = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>()
                .Create(Guid.NewGuid(), expectedCommand)
                .Build(Guid.NewGuid());

            var versionOneCommandQuery = new EntityVersionNumberQuery(1, 1);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>()
                .CreateRepository(TestSessionOptions.Write);

            // ACT

            var transactionInserted = await transactionRepository.PutTransaction(transaction);

            var newCommands = await transactionRepository.GetCommands(versionOneCommandQuery);

            // ASSERT

            transactionInserted.ShouldBeTrue();

            transaction.Steps.Length.ShouldBe(1);

            transaction.Steps[0].NextEntityVersionNumber.ShouldBe(1ul);

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

            var entityId = Guid.NewGuid();

            var transactionBuilder = serviceScope.ServiceProvider
                .GetRequiredService<TransactionBuilder<TransactionEntity>>();

            var firstTransaction = transactionBuilder
                .Create(entityId, new Count(1))
                .Build(Guid.NewGuid());

            var secondTransaction = transactionBuilder
                .Append(entityId, expectedCommand)
                .Build(Guid.NewGuid());

            var versionTwoCommandQuery = new EntityVersionNumberQuery(2, 2);

            await using var transactionRepository = await serviceScope.ServiceProvider
                .GetRequiredService<ITransactionRepositoryFactory<TransactionEntity>>().CreateRepository(TestSessionOptions.Write);

            var firstTransactionInserted = await transactionRepository.PutTransaction(firstTransaction);

            // ARRANGE ASSERTIONS

            firstTransactionInserted.ShouldBeTrue();

            // ACT

            var secondTransactionInserted = await transactionRepository.PutTransaction(secondTransaction);

            var newCommands = await transactionRepository.GetCommands(versionTwoCommandQuery);

            // ASSERT

            secondTransactionInserted.ShouldBeTrue();

            secondTransaction.Steps.Length.ShouldBe(1);

            secondTransaction.Steps[0].NextEntityVersionNumber.ShouldBe(2ul);

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

            var transactions = new List<ITransaction<TransactionEntity>>();
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

                var commands = new ICommand<TransactionEntity>[] { new Count(i) };

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

                var transaction = BuildTransaction(serviceScope, currentTransactionId, currentEntityId, commands,
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

            var transactions = new List<ITransaction<TransactionEntity>>();
            var expectedObjects = new ExpectedObjects();

            Guid? transactionId = null;

            var transactionIds = GetSortedGuids(numberOfTransactionIds);
            var entityIds = GetSortedGuids(numberOfTransactionIds);

            for (var i = 1; i <= numberOfTransactionIds; i++)
            {
                var currentTransactionId = transactionIds[i - 1];
                var currentEntityId = entityIds[i - 1];

                var agentSignature = new CounterAgentSignature(i);

                var commands = new ICommand<TransactionEntity>[] { new Count(i) };

                var leases = new[] { new CountLease(i) };

                var tags = new[] { new CountTag(i) };

                expectedObjects.Add(i == whichTransactionId, currentTransactionId, currentEntityId, agentSignature, commands,
                    leases, tags);

                if (i == whichTransactionId)
                {
                    transactionId = currentTransactionId;
                }

                var transaction = BuildTransaction(serviceScope, currentTransactionId, currentEntityId, commands,
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

            var transactions = new List<ITransaction<TransactionEntity>>();
            var expectedObjects = new ExpectedObjects();

            Guid? entityId = null;

            var transactionIds = GetSortedGuids(numberOfEntityIds);
            var entityIds = GetSortedGuids(numberOfEntityIds);

            for (var i = 1; i <= numberOfEntityIds; i++)
            {
                var currentTransactionId = transactionIds[i - 1];
                var currentEntityId = entityIds[i - 1];

                var agentSignature = new CounterAgentSignature(i);

                var commands = new ICommand<TransactionEntity>[] { new Count(i) };

                var leases = new[] { new CountLease(i) };

                var tags = new[] { new CountTag(i) };

                expectedObjects.Add(i == whichEntityId, currentTransactionId, currentEntityId, agentSignature, commands,
                    leases, tags);

                if (i == whichEntityId)
                {
                    entityId = currentEntityId;
                }

                var transaction = BuildTransaction(serviceScope, currentTransactionId, currentEntityId, commands,
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

            var commands = new List<ICommand<TransactionEntity>>();
            var expectedObjects = new ExpectedObjects();

            for (var i = 1; i <= numberOfVersionNumbers; i++)
            {
                var command = new Count(i);

                var leases = new[] { new CountLease(i) };

                var tags = new[] { new CountTag(i) };

                commands.Add(command);

                expectedObjects.Add(gteAsInt <= i && i <= lteAsInt, default, default, default!, new[] { command },
                    leases, tags);
            }

            var transaction = BuildTransaction(serviceScope, Guid.NewGuid(), Guid.NewGuid(), commands.ToArray());

            var transactions = new List<ITransaction<TransactionEntity>> { transaction };

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

            var transactions = new List<ITransaction<TransactionEntity>>();
            var expectedObjects = new ExpectedObjects();

            var transactionIds = GetSortedGuids(countTo);
            var entityIds = GetSortedGuids(countTo);

            for (var i = 1; i <= countTo; i++)
            {
                var currentTransactionId = transactionIds[i - 1];
                var currentEntityId = entityIds[i - 1];

                var agentSignature = new CounterAgentSignature(i);

                var commands = new ICommand<TransactionEntity>[] { new Count(i) };

                var leases = new[] { new CountLease(i) };

                var tags = new[] { new CountTag(i) };

                expectedObjects.Add(gte <= i && i <= lte, currentTransactionId, currentEntityId, agentSignature, commands,
                    leases, tags);

                var transaction = BuildTransaction(serviceScope, currentTransactionId, currentEntityId, commands,
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
            public readonly List<ICommand<TransactionEntity>> FalseCommands = new();
            public readonly List<Guid> FalseEntityIds = new();
            public readonly List<ILease> FalseLeases = new();
            public readonly List<object> FalseAgentSignatures = new();
            public readonly List<ITag> FalseTags = new();
            public readonly List<Guid> FalseTransactionIds = new();

            public readonly List<ICommand<TransactionEntity>> TrueCommands = new();
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
                IEnumerable<ICommand<TransactionEntity>> commands,
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
}

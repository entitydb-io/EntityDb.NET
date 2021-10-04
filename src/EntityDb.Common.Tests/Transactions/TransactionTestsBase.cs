using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Facts;
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
using EntityDb.Common.Transactions;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Entities;
using EntityDb.TestImplementations.Facts;
using EntityDb.TestImplementations.Leases;
using EntityDb.TestImplementations.Queries;
using EntityDb.TestImplementations.Source;
using EntityDb.TestImplementations.Tags;
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
    public abstract class TransactionTestsBase
    {
        private readonly IServiceProvider _serviceProvider;

        protected TransactionTestsBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private Task<ITransactionRepository<TransactionEntity>> CreateRepository(bool readOnly = false,
            bool tolerateLag = false, ILogger? loggerOverride = null)
        {
            return _serviceProvider.CreateTransactionRepository<TransactionEntity>(new TransactionSessionOptions
            {
                ReadOnly = readOnly, SecondaryPreferred = tolerateLag, LoggerOverride = loggerOverride
            });
        }

        private async Task TestGet<TResult>
        (
            List<ITransaction<TransactionEntity>> transactions,
            Func<bool, TResult[]> getExpectedResults,
            Func<ITransactionRepository<TransactionEntity>, bool, bool, int?, int?, Task<TResult[]>> getActualResults
        )
        {
            // ARRANGE

            TResult[]? expectedTrueResults = getExpectedResults.Invoke(false);
            TResult[]? expectedFalseResults = getExpectedResults.Invoke(true);
            TResult[]? reversedExpectedTrueResults = expectedTrueResults.Reverse().ToArray();
            TResult[]? reversedExpectedFalseResults = expectedFalseResults.Reverse().ToArray();
            IEnumerable<TResult>? expectedSkipTakeResults = expectedTrueResults.Skip(1).Take(1);

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            foreach (var transaction in transactions)
            {
                bool transactionInserted = await transactionRepository.PutTransaction(transaction);

                transactionInserted.ShouldBeTrue();
            }

            // ACT

            TResult[]? actualTrueResults =
                await getActualResults.Invoke(transactionRepository, false, false, null, null);
            TResult[]? actualFalseResults =
                await getActualResults.Invoke(transactionRepository, true, false, null, null);
            TResult[]? reversedActualTrueResults =
                await getActualResults.Invoke(transactionRepository, false, true, null, null);
            TResult[]? reversedActualFalseResults =
                await getActualResults.Invoke(transactionRepository, true, true, null, null);
            TResult[]? actualSkipTakeResults = await getActualResults.Invoke(transactionRepository, false, false, 1, 1);

            // ASSERT

            actualTrueResults.SequenceEqual(expectedTrueResults).ShouldBeTrue();
            actualFalseResults.SequenceEqual(expectedFalseResults).ShouldBeTrue();
            reversedActualTrueResults.SequenceEqual(reversedExpectedTrueResults).ShouldBeTrue();
            reversedActualFalseResults.SequenceEqual(reversedExpectedFalseResults).ShouldBeTrue();
            actualSkipTakeResults.SequenceEqual(expectedSkipTakeResults).ShouldBeTrue();
        }

        private Task TestGetTransactionIds(ISourceQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ISourceQuery, ISourceQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseTransactionIds : expectedObjects.TrueTransactionIds).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ISourceQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetTransactionIds(modifiedQuery);
                }
            );
        }

        private Task TestGetTransactionIds(ICommandQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ICommandQuery, ICommandQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseTransactionIds : expectedObjects.TrueTransactionIds).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ICommandQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetTransactionIds(modifiedQuery);
                }
            );
        }

        private Task TestGetTransactionIds(IFactQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<IFactQuery, IFactQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseTransactionIds : expectedObjects.TrueTransactionIds).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    IFactQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetTransactionIds(modifiedQuery);
                }
            );
        }

        private Task TestGetTransactionIds(ILeaseQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ILeaseQuery, ILeaseQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseTransactionIds : expectedObjects.TrueTransactionIds).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ILeaseQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetTransactionIds(modifiedQuery);
                }
            );
        }

        private Task TestGetEntityIds(ISourceQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ISourceQuery, ISourceQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseEntityIds : expectedObjects.TrueEntityIds).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ISourceQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetEntityIds(modifiedQuery);
                }
            );
        }

        private Task TestGetEntityIds(ICommandQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ICommandQuery, ICommandQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseEntityIds : expectedObjects.TrueEntityIds).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ICommandQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetEntityIds(modifiedQuery);
                }
            );
        }

        private Task TestGetEntityIds(IFactQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<IFactQuery, IFactQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseEntityIds : expectedObjects.TrueEntityIds).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    IFactQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetEntityIds(modifiedQuery);
                }
            );
        }

        private Task TestGetEntityIds(ILeaseQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ILeaseQuery, ILeaseQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseEntityIds : expectedObjects.TrueEntityIds).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ILeaseQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetEntityIds(modifiedQuery);
                }
            );
        }

        private Task TestGetSources(ISourceQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ISourceQuery, ISourceQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseSources : expectedObjects.TrueSources).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ISourceQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetSources(modifiedQuery);
                }
            );
        }

        private Task TestGetCommands(ICommandQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ICommandQuery, ICommandQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseCommands : expectedObjects.TrueCommands).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ICommandQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetCommands(modifiedQuery);
                }
            );
        }

        private Task TestGetFacts(IFactQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<IFactQuery, IFactQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseFacts : expectedObjects.TrueFacts).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    IFactQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetFacts(modifiedQuery);
                }
            );
        }

        private Task TestGetLeases(ILeaseQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ILeaseQuery, ILeaseQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseLeases : expectedObjects.TrueLeases).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ILeaseQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetLeases(modifiedQuery);
                }
            );
        }

        private Task TestGetTags(ITagQuery query, List<ITransaction<TransactionEntity>> transactions,
            ExpectedObjects expectedObjects, Func<ITagQuery, ITagQuery>? filter = null)
        {
            return TestGet
            (
                transactions,
                invert => (invert ? expectedObjects.FalseTags : expectedObjects.TrueTags).ToArray(),
                (transactionRepository, invertFilter, reverseSort, skip, take) =>
                {
                    ModifiedQueryOptions? modifiedQueryOptions = new()
                    {
                        InvertFilter = invertFilter,
                        ReverseSort = reverseSort,
                        ReplaceSkip = skip,
                        ReplaceTake = take
                    };

                    ITagQuery? modifiedQuery = query.Modify(modifiedQueryOptions);

                    if (filter != null)
                    {
                        modifiedQuery = filter.Invoke(modifiedQuery);
                    }

                    return transactionRepository.GetTags(modifiedQuery);
                }
            );
        }

        private ITransaction<TransactionEntity> BuildTransaction(Guid transactionId, Guid entityId, object source,
            ICommand<TransactionEntity>[] commands, DateTime? timeStampOverride = null)
        {
            TransactionBuilder<TransactionEntity>? transactionBuilder =
                _serviceProvider.GetTransactionBuilder<TransactionEntity>();

            transactionBuilder.Create(entityId, commands[0]);

            for (int i = 1; i < commands.Length; i++)
            {
                transactionBuilder.Append(entityId, commands[i]);
            }

            return transactionBuilder.Build(transactionId, source, timeStampOverride);
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
        public async Task GivenReadOnlyMode_WhenPuttingTransaction_ThenThrow()
        {
            // ARRANGE

            Transaction<TransactionEntity>? transaction = new()
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                Source = new NoSource(),
                Commands = new[]
                {
                    new TransactionCommand<TransactionEntity>
                    {
                        PreviousSnapshot = default,
                        NextSnapshot = default!,
                        EntityId = Guid.NewGuid(),
                        ExpectedPreviousVersionNumber = 0,
                        Command = new DoNothing(),
                        Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                        Leases = new TransactionMetaData<ILease>(),
                        Tags = new TransactionMetaData<ITag>()
                    }
                }.ToImmutableArray<ITransactionCommand<TransactionEntity>>()
            };

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository(true);

            // ASSERT

            await Should.ThrowAsync<CannotWriteInReadOnlyModeException>(async () =>
                await transactionRepository.PutTransaction(transaction));
        }

        [Fact]
        public async Task GivenNonUniqueTransactionIds_WhenPuttingTransactions_ThenSecondPutReturnsFalse()
        {
            // ARRANGE

            Guid transactionId = Guid.NewGuid();

            static ITransaction<TransactionEntity> NewTransaction(Guid transactionId)
            {
                return new Transaction<TransactionEntity>
                {
                    Id = transactionId,
                    TimeStamp = DateTime.UtcNow,
                    Source = new NoSource(),
                    Commands = new[]
                    {
                        new TransactionCommand<TransactionEntity>
                        {
                            PreviousSnapshot = default,
                            NextSnapshot = default!,
                            EntityId = Guid.NewGuid(),
                            ExpectedPreviousVersionNumber = 0,
                            Command = new DoNothing(),
                            Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                            Leases = new TransactionMetaData<ILease>(),
                            Tags = new TransactionMetaData<ITag>()
                        }
                    }.ToImmutableArray<ITransactionCommand<TransactionEntity>>()
                };
            }

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            // ACT

            bool firstTransactionInserted = await transactionRepository.PutTransaction(NewTransaction(transactionId));
            bool secondTransactionInserted = await transactionRepository.PutTransaction(NewTransaction(transactionId));

            // ASSERT

            firstTransactionInserted.ShouldBeTrue();
            secondTransactionInserted.ShouldBeFalse();
        }

        [Fact]
        public async Task GivenNonUniqueVersionNumbers_WhenInsertingCommands_ThenReturnFalse()
        {
            // ARRANGE

            Guid entityId = Guid.NewGuid();
            ulong previousVersionNumber = 0;

            Transaction<TransactionEntity>? transaction = new()
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                Source = new NoSource(),
                Commands = new[]
                {
                    new TransactionCommand<TransactionEntity>
                    {
                        PreviousSnapshot = default,
                        NextSnapshot = default!,
                        EntityId = entityId,
                        ExpectedPreviousVersionNumber = previousVersionNumber,
                        Command = new DoNothing(),
                        Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                        Leases = new TransactionMetaData<ILease>(),
                        Tags = new TransactionMetaData<ITag>()
                    },
                    new TransactionCommand<TransactionEntity>
                    {
                        PreviousSnapshot = default,
                        NextSnapshot = default!,
                        EntityId = entityId,
                        ExpectedPreviousVersionNumber = previousVersionNumber,
                        Command = new DoNothing(),
                        Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                        Leases = new TransactionMetaData<ILease>(),
                        Tags = new TransactionMetaData<ITag>()
                    }
                }.ToImmutableArray<ITransactionCommand<TransactionEntity>>()
            };

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            // ACT

            bool transactionInserted = await transactionRepository.PutTransaction(transaction);

            // ASSERT

            transactionInserted.ShouldBeFalse();
        }

        [Fact]
        public async Task
            GivenNonUniqueVersionNumbers_WhenInsertingCommands_ThenOptimisticConcurrencyExceptionIsLogged()
        {
            // ARRANGE

            const ulong previousVersionNumber = 0;

            Guid entityId = Guid.NewGuid();

            static ITransaction<TransactionEntity> NewTransaction(Guid entityId, ulong previousVersionNumber)
            {
                return new Transaction<TransactionEntity>
                {
                    Id = Guid.NewGuid(),
                    TimeStamp = DateTime.UtcNow,
                    Source = new NoSource(),
                    Commands = new[]
                    {
                        new TransactionCommand<TransactionEntity>
                        {
                            PreviousSnapshot = default,
                            NextSnapshot = default!,
                            EntityId = entityId,
                            ExpectedPreviousVersionNumber = previousVersionNumber,
                            Command = new DoNothing(),
                            Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                            Leases = new TransactionMetaData<ILease>(),
                            Tags = new TransactionMetaData<ITag>()
                        }
                    }.ToImmutableArray<ITransactionCommand<TransactionEntity>>()
                };
            }

            Mock<ILogger>? loggerMock = new(MockBehavior.Strict);

            loggerMock
                .Setup(logger => logger.LogError(It.IsAny<OptimisticConcurrencyException>(), It.IsAny<string>()))
                .Verifiable();

            await using ITransactionRepository<TransactionEntity>? transactionRepository =
                await CreateRepository(loggerOverride: loggerMock.Object);

            // ACT

            bool firstTransactionInserted =
                await transactionRepository.PutTransaction(NewTransaction(entityId, previousVersionNumber));
            bool secondTransactionInserted =
                await transactionRepository.PutTransaction(NewTransaction(entityId, previousVersionNumber));

            // ASSERT

            firstTransactionInserted.ShouldBeTrue();
            secondTransactionInserted.ShouldBeFalse();

            loggerMock.Verify();
        }

        [Fact]
        public async Task GivenNonUniqueSubversionNumbers_WhenInsertingFacts_ThenReturnFalse()
        {
            // ARRANGE

            Guid entityId = Guid.NewGuid();
            ulong subversionNumber = 0;

            Transaction<TransactionEntity>? transaction = new()
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                Source = new NoSource(),
                Commands = new[]
                {
                    new TransactionCommand<TransactionEntity>
                    {
                        PreviousSnapshot = default,
                        NextSnapshot = default!,
                        EntityId = entityId,
                        ExpectedPreviousVersionNumber = 0,
                        Command = new DoNothing(),
                        Facts = new[]
                        {
                            new TransactionFact<TransactionEntity>
                            {
                                SubversionNumber = subversionNumber, Fact = new NothingDone()
                            },
                            new TransactionFact<TransactionEntity>
                            {
                                SubversionNumber = subversionNumber, Fact = new NothingDone()
                            }
                        }.ToImmutableArray<ITransactionFact<TransactionEntity>>(),
                        Leases = new TransactionMetaData<ILease>(),
                        Tags = new TransactionMetaData<ITag>()
                    }
                }.ToImmutableArray<ITransactionCommand<TransactionEntity>>()
            };

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            // ACT

            bool transactionInserted = await transactionRepository.PutTransaction(transaction);

            // ASSERT

            transactionInserted.ShouldBeFalse();
        }

        [Fact]
        public async Task GivenNonUniqueTags_WhenInsertingTagDocuments_ThenReturnTrue()
        {
            // ARRANGE

            Tag? tag = new("Foo", "Bar");

            Transaction<TransactionEntity>? transaction = new()
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                Source = new NoSource(),
                Commands = new[]
                {
                    new TransactionCommand<TransactionEntity>
                    {
                        PreviousSnapshot = default,
                        NextSnapshot = default!,
                        EntityId = Guid.NewGuid(),
                        ExpectedPreviousVersionNumber = 0,
                        Command = new DoNothing(),
                        Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                        Leases = new TransactionMetaData<ILease>(),
                        Tags = new TransactionMetaData<ITag>
                        {
                            Insert = new[] { tag }.ToImmutableArray<ITag>()
                        }
                    },
                    new TransactionCommand<TransactionEntity>
                    {
                        PreviousSnapshot = default,
                        NextSnapshot = default!,
                        EntityId = Guid.NewGuid(),
                        ExpectedPreviousVersionNumber = 0,
                        Command = new DoNothing(),
                        Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                        Leases = new TransactionMetaData<ILease>(),
                        Tags = new TransactionMetaData<ITag>
                        {
                            Insert = new[] { tag }.ToImmutableArray<ITag>()
                        }
                    }
                }.ToImmutableArray<ITransactionCommand<TransactionEntity>>()
            };

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            // ACT

            bool transactionInserted = await transactionRepository.PutTransaction(transaction);

            // ASSERT

            transactionInserted.ShouldBeTrue();
        }

        [Fact]
        public async Task GivenNonUniqueLeases_WhenInsertingLeaseDocuments_ThenReturnFalse()
        {
            // ARRANGE

            Lease? lease = new("Foo", "Bar", "Baz");

            Transaction<TransactionEntity>? transaction = new()
            {
                Id = Guid.NewGuid(),
                TimeStamp = DateTime.UtcNow,
                Source = new NoSource(),
                Commands = new[]
                {
                    new TransactionCommand<TransactionEntity>
                    {
                        PreviousSnapshot = default,
                        NextSnapshot = default!,
                        EntityId = Guid.NewGuid(),
                        ExpectedPreviousVersionNumber = 0,
                        Command = new DoNothing(),
                        Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                        Leases = new TransactionMetaData<ILease>
                        {
                            Insert = new[] { lease }.ToImmutableArray<ILease>()
                        },
                        Tags = new TransactionMetaData<ITag>()
                    },
                    new TransactionCommand<TransactionEntity>
                    {
                        PreviousSnapshot = default,
                        NextSnapshot = default!,
                        EntityId = Guid.NewGuid(),
                        ExpectedPreviousVersionNumber = 0,
                        Command = new DoNothing(),
                        Facts = ImmutableArray<ITransactionFact<TransactionEntity>>.Empty,
                        Leases = new TransactionMetaData<ILease>
                        {
                            Insert = new[] { lease }.ToImmutableArray<ILease>()
                        },
                        Tags = new TransactionMetaData<ITag>()
                    }
                }.ToImmutableArray<ITransactionCommand<TransactionEntity>>()
            };

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            // ACT

            bool transactionInserted = await transactionRepository.PutTransaction(transaction);

            // ASSERT

            transactionInserted.ShouldBeFalse();
        }

        [Fact]
        public async Task GivenEntityInserted_WhenGettingEntity_ThenReturnEntity()
        {
            // ARRANGE

            TransactionEntity? expectedEntity = new() { VersionNumber = 1 };

            Guid entityId = Guid.NewGuid();

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            EntityRepository<TransactionEntity>? entityRepository = new(_serviceProvider, transactionRepository);

            ITransaction<TransactionEntity>? transaction =
                BuildTransaction(Guid.NewGuid(), entityId, new NoSource(), new[] { new DoNothing() });

            await transactionRepository.PutTransaction(transaction);

            // ACT

            TransactionEntity? actualEntity = await entityRepository.Get(entityId);

            // ASSERT

            actualEntity.ShouldBeEquivalentTo(expectedEntity);
        }

        [Fact]
        public async Task GivenEntityInsertedWithTags_WhenRemovingAllTags_ThenFinalEntityHasNoTags()
        {
            // ARRANGE

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                _serviceProvider.GetTransactionBuilder<TransactionEntity>();

            ImmutableArray<ITag> expectedInitialTags = new[] { new Tag("Foo", "Bar") }.ToImmutableArray<ITag>();

            Guid entityId = Guid.NewGuid();

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            ITransaction<TransactionEntity>? initialTransaction = transactionBuilder
                .Create(entityId, new AddTag("Foo", "Bar"))
                .Build(Guid.NewGuid(), new NoSource());

            await transactionRepository.PutTransaction(initialTransaction);

            DeleteTagsQuery? tagQuery = new(entityId, expectedInitialTags);

            // ACT

            ITag[]? actualInitialTags = await transactionRepository.GetTags(tagQuery);

            ITransaction<TransactionEntity>? finalTransaction = transactionBuilder
                .Append(entityId, new RemoveAllTags())
                .Build(Guid.NewGuid(), new NoSource());

            await transactionRepository.PutTransaction(finalTransaction);

            ITag[]? actualFinalTags = await transactionRepository.GetTags(tagQuery);

            // ASSERT

            expectedInitialTags.SequenceEqual(actualInitialTags).ShouldBeTrue();

            actualFinalTags.ShouldBeEmpty();
        }

        [Fact]
        public async Task GivenEntityInsertedWithLeases_WhenRemovingAllLeases_ThenFinalEntityHasNoLeases()
        {
            // ARRANGE

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                _serviceProvider.GetTransactionBuilder<TransactionEntity>();

            ImmutableArray<ILease> expectedInitialLeases =
                new[] { new Lease("Foo", "Bar", "Baz") }.ToImmutableArray<ILease>();

            Guid entityId = Guid.NewGuid();

            await using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            ITransaction<TransactionEntity>? initialTransaction = transactionBuilder
                .Create(entityId, new AddLease("Foo", "Bar", "Baz"))
                .Build(Guid.NewGuid(), new NoSource());

            await transactionRepository.PutTransaction(initialTransaction);

            DeleteLeasesQuery? leaseQuery = new(entityId, expectedInitialLeases);

            // ACT

            ILease[]? actualInitialLeases = await transactionRepository.GetLeases(leaseQuery);

            ITransaction<TransactionEntity>? finalTransaction = transactionBuilder
                .Append(entityId, new RemoveAllLeases())
                .Build(Guid.NewGuid(), new NoSource());

            await transactionRepository.PutTransaction(finalTransaction);

            ILease[]? actualFinalLeases = await transactionRepository.GetLeases(leaseQuery);

            // ASSERT

            actualInitialLeases.SequenceEqual(expectedInitialLeases).ShouldBeTrue();

            actualFinalLeases.ShouldBeEmpty();
        }

        [Fact]
        public async Task GivenTransactionCreatesEntity_WhenQueryingForVersionOne_ThenReturnTheExpectedCommand()
        {
            // ARRANGE

            Count? expectedCommand = new(1);

            ITransaction<TransactionEntity>? transaction = _serviceProvider
                .GetTransactionBuilder<TransactionEntity>()
                .Create(Guid.NewGuid(), expectedCommand)
                .Build(Guid.NewGuid(), new NoSource());

            EntityVersionNumberQuery? versionOneCommandQuery = new(1, 1);

            using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            // ACT

            await transactionRepository.PutTransaction(transaction);

            ICommand<TransactionEntity>[]?
                newCommands = await transactionRepository.GetCommands(versionOneCommandQuery);

            // ASSERT

            transaction.Commands.Length.ShouldBe(1);

            transaction.Commands[0].ExpectedPreviousVersionNumber.ShouldBe(default);

            newCommands.Length.ShouldBe(1);

            newCommands[0].ShouldBeEquivalentTo(expectedCommand);
        }

        [Fact]
        public async Task
            GivenTransactionAppendsEntityWithOneVersion_WhenQueryingForVersionTwo_ThenReturnExpectedCommand()
        {
            // ARRANGE

            Count? expectedCommand = new(2);

            Guid entityId = Guid.NewGuid();

            TransactionBuilder<TransactionEntity>? transactionBuilder =
                _serviceProvider.GetTransactionBuilder<TransactionEntity>();

            ITransaction<TransactionEntity>? firstTransaction = transactionBuilder
                .Create(entityId, new Count(1))
                .Build(Guid.NewGuid(), new NoSource());

            ITransaction<TransactionEntity>? secondTransaction = transactionBuilder
                .Append(entityId, expectedCommand)
                .Build(Guid.NewGuid(), new NoSource());

            EntityVersionNumberQuery? versionTwoCommandQuery = new(2, 2);

            using ITransactionRepository<TransactionEntity>? transactionRepository = await CreateRepository();

            await transactionRepository.PutTransaction(firstTransaction);

            // ACT

            await transactionRepository.PutTransaction(secondTransaction);

            ICommand<TransactionEntity>[]?
                newCommands = await transactionRepository.GetCommands(versionTwoCommandQuery);

            // ASSERT

            secondTransaction.Commands.Length.ShouldBe(1);

            secondTransaction.Commands[0].ExpectedPreviousVersionNumber.ShouldBe(1ul);

            newCommands.Length.ShouldBe(1);

            newCommands[0].ShouldBeEquivalentTo(expectedCommand);
        }

        [Theory]
        [InlineData(60, 20, 30)]
        public async Task GivenTransactionAlreadyInserted_WhenQueryingByTransactionTimeStamp_ThenReturnExpectedObjects(
            int timeSpanInMinutes, int gteInMinutes, int lteInMinutes)
        {
            DateTime originTimeStamp = DateTime.UnixEpoch;

            List<ITransaction<TransactionEntity>>? transactions = new();
            ExpectedObjects? expectedObjects = new();

            Guid[]? transactionIds = GetSortedGuids(timeSpanInMinutes);
            Guid[]? entityIds = GetSortedGuids(timeSpanInMinutes);

            DateTime? gte = null;
            DateTime? lte = null;

            for (int i = 1; i <= timeSpanInMinutes; i++)
            {
                Guid currentTransactionId = transactionIds[i - 1];
                Guid currentEntityId = entityIds[i - 1];

                DateTime currentTimeStamp = originTimeStamp.AddMinutes(i);

                Counter? source = new(i);

                ICommand<TransactionEntity>[]? commands = { new Count(i) };

                IFact<TransactionEntity>[]? facts =
                {
                    new Counted(i), _serviceProvider.GetVersionNumberFact<TransactionEntity>(1)
                };

                CountLease[]? leases = { new(i) };

                CountTag[]? tags = { new(i) };

                expectedObjects.Add(gteInMinutes <= i && i <= lteInMinutes, currentTransactionId, currentEntityId,
                    source, commands, facts, leases, tags);

                if (i == lteInMinutes)
                {
                    lte = currentTimeStamp;
                }
                else if (i == gteInMinutes)
                {
                    gte = currentTimeStamp;
                }

                ITransaction<TransactionEntity>? transaction = BuildTransaction(currentTransactionId, currentEntityId,
                    source, commands, currentTimeStamp);

                transactions.Add(transaction);
            }

            gte.ShouldNotBeNull();
            lte.ShouldNotBeNull();

            TransactionTimeStampQuery? query = new(gte!.Value, lte!.Value);

            await TestGetTransactionIds(query as ISourceQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as ICommandQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as IFactQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as ILeaseQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ISourceQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ICommandQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as IFactQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ILeaseQuery, transactions, expectedObjects);
            await TestGetSources(query, transactions, expectedObjects);
            await TestGetCommands(query, transactions, expectedObjects);
            await TestGetFacts(query, transactions, expectedObjects);
            await TestGetLeases(query, transactions, expectedObjects);
            await TestGetTags(query, transactions, expectedObjects);
        }

        [Theory]
        [InlineData(10, 5)]
        public async Task GivenTransactionAlreadyInserted_WhenQueryingByTransactionId_ThenReturnExpectedObjects(
            int numberOfTransactionIds, int whichTransactionId)
        {
            List<ITransaction<TransactionEntity>>? transactions = new();
            ExpectedObjects? expectedObjects = new();

            Guid? transactionId = null;

            Guid[]? transactionIds = GetSortedGuids(numberOfTransactionIds);
            Guid[]? entityIds = GetSortedGuids(numberOfTransactionIds);

            for (int i = 1; i <= numberOfTransactionIds; i++)
            {
                Guid currentTransactionId = transactionIds[i - 1];
                Guid currentEntityId = entityIds[i - 1];

                Counter? source = new(i);

                ICommand<TransactionEntity>[]? commands = { new Count(i) };

                IFact<TransactionEntity>[]? facts =
                {
                    new Counted(i), _serviceProvider.GetVersionNumberFact<TransactionEntity>(1)
                };

                CountLease[]? leases = { new(i) };

                CountTag[]? tags = { new(i) };

                expectedObjects.Add(i == whichTransactionId, currentTransactionId, currentEntityId, source, commands,
                    facts, leases, tags);

                if (i == whichTransactionId)
                {
                    transactionId = currentTransactionId;
                }

                ITransaction<TransactionEntity>? transaction =
                    BuildTransaction(currentTransactionId, currentEntityId, source, commands);

                transactions.Add(transaction);
            }

            transactionId.ShouldNotBeNull();

            TransactionIdQuery? query = new(transactionId!.Value);

            await TestGetTransactionIds(query as ISourceQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as ICommandQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as IFactQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as ILeaseQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ISourceQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ICommandQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as IFactQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ILeaseQuery, transactions, expectedObjects);
            await TestGetSources(query, transactions, expectedObjects);
            await TestGetCommands(query, transactions, expectedObjects);
            await TestGetFacts(query, transactions, expectedObjects);
            await TestGetLeases(query, transactions, expectedObjects);
            await TestGetTags(query, transactions, expectedObjects);
        }

        [Theory]
        [InlineData(10, 5)]
        public async Task GivenTransactionAlreadyInserted_WhenQueryingByEntityId_ThenReturnExpectedObjects(
            int numberOfEntityIds, int whichEntityId)
        {
            List<ITransaction<TransactionEntity>>? transactions = new();
            ExpectedObjects? expectedObjects = new();

            Guid? entityId = null;

            Guid[]? transactionIds = GetSortedGuids(numberOfEntityIds);
            Guid[]? entityIds = GetSortedGuids(numberOfEntityIds);

            for (int i = 1; i <= numberOfEntityIds; i++)
            {
                Guid currentTransactionId = transactionIds[i - 1];
                Guid currentEntityId = entityIds[i - 1];

                Counter? source = new(i);

                ICommand<TransactionEntity>[]? commands = { new Count(i) };

                IFact<TransactionEntity>[]? facts =
                {
                    new Counted(i), _serviceProvider.GetVersionNumberFact<TransactionEntity>(1)
                };

                CountLease[]? leases = { new(i) };

                CountTag[]? tags = { new(i) };

                expectedObjects.Add(i == whichEntityId, currentTransactionId, currentEntityId, source, commands, facts,
                    leases, tags);

                if (i == whichEntityId)
                {
                    entityId = currentEntityId;
                }

                ITransaction<TransactionEntity>? transaction =
                    BuildTransaction(currentTransactionId, currentEntityId, source, commands);

                transactions.Add(transaction);
            }

            entityId.ShouldNotBeNull();

            EntityIdQuery? query = new(entityId!.Value);

            await TestGetTransactionIds(query as ISourceQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as ICommandQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as IFactQuery, transactions, expectedObjects);
            await TestGetTransactionIds(query as ILeaseQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ISourceQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ICommandQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as IFactQuery, transactions, expectedObjects);
            await TestGetEntityIds(query as ILeaseQuery, transactions, expectedObjects);
            await TestGetSources(query, transactions, expectedObjects);
            await TestGetCommands(query, transactions, expectedObjects);
            await TestGetFacts(query, transactions, expectedObjects);
            await TestGetLeases(query, transactions, expectedObjects);
            await TestGetTags(query, transactions, expectedObjects);
        }

        [Theory]
        [InlineData(20, 5, 15)]
        public async Task GivenTransactionAlreadyInserted_WhenQueryingByEntityVersionNumber_ThenReturnExpectedObjects(
            int numberOfVersionNumbers, int gteAsInt, int lteAsInt)
        {
            List<ICommand<TransactionEntity>>? commands = new();
            ExpectedObjects? expectedObjects = new();

            for (int i = 1; i <= numberOfVersionNumbers; i++)
            {
                Count? command = new(i);

                IFact<TransactionEntity>[]? facts =
                {
                    new Counted(i), _serviceProvider.GetVersionNumberFact<TransactionEntity>((ulong)i)
                };

                CountLease[]? leases = { new(i) };

                CountTag[]? tags = { new(i) };

                commands.Add(command);

                expectedObjects.Add(gteAsInt <= i && i <= lteAsInt, default, default, default!, new[] { command },
                    facts, leases, tags);
            }

            ITransaction<TransactionEntity>? transaction =
                BuildTransaction(Guid.NewGuid(), Guid.NewGuid(), new NoSource(), commands.ToArray());

            List<ITransaction<TransactionEntity>>? transactions = new() { transaction };

            EntityVersionNumberQuery? query = new((ulong)gteAsInt, (ulong)lteAsInt);

            await TestGetCommands(query, transactions, expectedObjects);
            await TestGetFacts(query, transactions, expectedObjects);
            await TestGetLeases(query, transactions, expectedObjects);
            await TestGetTags(query, transactions, expectedObjects);
        }

        [Theory]
        [InlineData(20, 5, 15)]
        public async Task GivenTransactionAlreadyInserted_WhenQueryingByData_ThenReturnExpectedObjects(int countTo,
            int gte, int lte)
        {
            List<ITransaction<TransactionEntity>>? transactions = new();
            ExpectedObjects? expectedObjects = new();

            Guid[]? transactionIds = GetSortedGuids(countTo);
            Guid[]? entityIds = GetSortedGuids(countTo);

            for (int i = 1; i <= countTo; i++)
            {
                Guid currentTransactionId = transactionIds[i - 1];
                Guid currentEntityId = entityIds[i - 1];

                Counter? source = new(i);

                ICommand<TransactionEntity>[]? commands = { new Count(i) };

                IFact<TransactionEntity>[]? facts = { new Counted(i) };

                CountLease[]? leases = { new(i) };

                CountTag[]? tags = { new(i) };

                expectedObjects.Add(gte <= i && i <= lte, currentTransactionId, currentEntityId, source, commands,
                    facts, leases, tags);

                ITransaction<TransactionEntity>? transaction =
                    BuildTransaction(currentTransactionId, currentEntityId, source, commands);

                transactions.Add(transaction);
            }

            ISourceQuery FilterSources(ISourceQuery sourceQuery)
            {
                return sourceQuery.Filter(new CountFilter());
            }

            ICommandQuery FilterCommands(ICommandQuery commandQuery)
            {
                return commandQuery.Filter(new CountFilter());
            }

            IFactQuery FilterFacts(IFactQuery factQuery)
            {
                return factQuery.Filter(new CountFilter());
            }

            ILeaseQuery FilterLeases(ILeaseQuery leaseQuery)
            {
                return leaseQuery.Filter(new CountFilter());
            }

            ITagQuery FilterTags(ITagQuery tagQuery)
            {
                return tagQuery.Filter(new CountFilter());
            }

            CountQuery<TransactionEntity>? query = new(gte, lte);

            await TestGetTransactionIds(query, transactions, expectedObjects, FilterSources);
            await TestGetTransactionIds(query, transactions, expectedObjects, FilterCommands);
            await TestGetTransactionIds(query, transactions, expectedObjects, FilterFacts);
            await TestGetTransactionIds(query, transactions, expectedObjects, FilterLeases);
            await TestGetEntityIds(query, transactions, expectedObjects, FilterSources);
            await TestGetEntityIds(query, transactions, expectedObjects, FilterCommands);
            await TestGetEntityIds(query, transactions, expectedObjects, FilterFacts);
            await TestGetEntityIds(query, transactions, expectedObjects, FilterLeases);
            await TestGetSources(query, transactions, expectedObjects, FilterSources);
            await TestGetCommands(query, transactions, expectedObjects, FilterCommands);
            await TestGetFacts(query, transactions, expectedObjects, FilterFacts);
            await TestGetLeases(query, transactions, expectedObjects, FilterLeases);
            await TestGetTags(query, transactions, expectedObjects, FilterTags);
        }

        private class ExpectedObjects
        {
            public readonly List<ICommand<TransactionEntity>> FalseCommands = new();
            public readonly List<Guid> FalseEntityIds = new();
            public readonly List<IFact<TransactionEntity>> FalseFacts = new();
            public readonly List<ILease> FalseLeases = new();
            public readonly List<object> FalseSources = new();
            public readonly List<ITag> FalseTags = new();
            public readonly List<Guid> FalseTransactionIds = new();

            public readonly List<ICommand<TransactionEntity>> TrueCommands = new();

            public readonly List<Guid> TrueEntityIds = new();

            public readonly List<IFact<TransactionEntity>> TrueFacts = new();

            public readonly List<ILease> TrueLeases = new();

            public readonly List<object> TrueSources = new();

            public readonly List<ITag> TrueTags = new();
            public readonly List<Guid> TrueTransactionIds = new();

            public void Add
            (
                bool condition,
                Guid transactionId,
                Guid entityId,
                object source,
                IEnumerable<ICommand<TransactionEntity>> commands,
                IEnumerable<IFact<TransactionEntity>> facts,
                IEnumerable<ILease> leases,
                IEnumerable<ITag> tags
            )
            {
                if (condition)
                {
                    TrueTransactionIds.Add(transactionId);
                    TrueEntityIds.Add(entityId);
                    TrueSources.Add(source);
                    TrueCommands.AddRange(commands);
                    TrueFacts.AddRange(facts);
                    TrueLeases.AddRange(leases);
                    TrueTags.AddRange(tags);
                }
                else
                {
                    FalseTransactionIds.Add(transactionId);
                    FalseEntityIds.Add(entityId);
                    FalseSources.Add(source);
                    FalseCommands.AddRange(commands);
                    FalseFacts.AddRange(facts);
                    FalseLeases.AddRange(leases);
                    FalseTags.AddRange(tags);
                }
            }
        }
    }
}

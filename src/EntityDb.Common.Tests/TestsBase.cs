using EntityDb.Abstractions.Facts;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Transactions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace EntityDb.Common.Tests
{
    public class TestsBase
    {
        protected readonly IServiceProvider _serviceProvider;

        public TestsBase(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private IServiceCollection GetParaisteServiceCollection(Type[]? omittedTypes = null)
        {
            var serviceCollection = new ServiceCollection();
            
            // Use reflection to get all service descriptors
            
            var engine = _serviceProvider.GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Single(x => x.Name == "Engine")
                .GetValue(_serviceProvider);

            var callSiteFactory = engine!.GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(x => x.Name == "CallSiteFactory")
                .GetValue(engine);
            
            var descriptors = (callSiteFactory!.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Single(x => x.Name == "_descriptors")
                .GetValue(callSiteFactory) as List<ServiceDescriptor>)!;
            
            foreach (var descriptor in descriptors)
            {
                if (omittedTypes?.Contains(descriptor.ServiceType) == true)
                {
                    continue;
                }
                
                serviceCollection.Add(descriptor);
            }

            return serviceCollection;
        }
        
        public IServiceProvider GetServiceProviderWithOverrides(Action<IServiceCollection> configureOverrides)
        {
            var serviceCollection = GetParaisteServiceCollection();

            configureOverrides.Invoke(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        public IServiceProvider GetServiceProviderWithOmission<TOmittedType>()
        {
            return GetParaisteServiceCollection(new[] { typeof(TOmittedType) }).BuildServiceProvider();
        }

        public static ITransactionRepositoryFactory<TEntity> GetMockedTransactionRepositoryFactory<TEntity>(
            IFact<TEntity>[]? facts = null)
        {
            if (facts == null)
            {
                facts = Array.Empty<IFact<TEntity>>();
            }

            var transactionRepositoryMock = new Mock<ITransactionRepository<TEntity>>(MockBehavior.Strict);

            transactionRepositoryMock
                .Setup(session => session.PutTransaction(It.IsAny<ITransaction<TEntity>>()))
                .ReturnsAsync(true)
                .Verifiable();

            transactionRepositoryMock
                .Setup(repository => repository.GetFacts(It.IsAny<IFactQuery>()))
                .ReturnsAsync(facts);

            transactionRepositoryMock
                .Setup(repository => repository.DisposeAsync())
                .Returns(ValueTask.CompletedTask);

            var transactionRepositoryFactoryMock =
                new Mock<ITransactionRepositoryFactory<TEntity>>(MockBehavior.Strict);

            transactionRepositoryFactoryMock
                .Setup(factory => factory.CreateRepository(It.IsAny<ITransactionSessionOptions>()))
                .ReturnsAsync(transactionRepositoryMock.Object);

            return transactionRepositoryFactoryMock.Object;
        }
    }
}

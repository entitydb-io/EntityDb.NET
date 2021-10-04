using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Tags;
using System;
using System.Threading.Tasks;

namespace EntityDb.Void.Transactions
{
    internal class VoidTransactionRepository
    {
        protected static readonly Task<Guid[]> EmptyGuidArrayTask = Task.FromResult(Array.Empty<Guid>());
        protected static readonly Task<object[]> EmptyObjectArrayTask = Task.FromResult(Array.Empty<object>());
        protected static readonly Task<ILease[]> EmptyLeaseArrayTask = Task.FromResult(Array.Empty<ILease>());
        protected static readonly Task<ITag[]> EmptyTagArrayTask = Task.FromResult(Array.Empty<ITag>());
        protected static readonly Task<bool> TrueTask = Task.FromResult(true);
    }
}

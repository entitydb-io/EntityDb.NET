using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Transactions.Builders;
using System.Threading.Tasks;

namespace EntityDb.Abstractions.Transactions.Builders;

public interface ITransactionBuilderFactory<TEntity>
{
    Task<ITransactionBuilder<TEntity>> Create(string agentSignatureOptionsName);
    Task<ISingleEntityTransactionBuilder<TEntity>> CreateForSingleEntity(string agentSignatureOptionsName, Id entityId);
}

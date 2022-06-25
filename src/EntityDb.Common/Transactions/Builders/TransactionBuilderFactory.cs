using EntityDb.Abstractions.Agents;
using EntityDb.Abstractions.Transactions.Builders;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.Common.Transactions.Builders;

internal sealed class TransactionBuilderFactory<TEntity> : ITransactionBuilderFactory<TEntity>
    where TEntity : IEntity<TEntity>
{
    private readonly IAgentAccessor _agentAccessor;

    public TransactionBuilderFactory(IAgentAccessor agentAccessor)
    {
        _agentAccessor = agentAccessor;
    }

    public async Task<ITransactionBuilder<TEntity>> Create(string agentSignatureOptionsName, CancellationToken cancellationToken)
    {
        var agent = await _agentAccessor.GetAgentAsync(agentSignatureOptionsName, cancellationToken);

        return new TransactionBuilder<TEntity>(agent);
    }

    public async Task<ISingleEntityTransactionBuilder<TEntity>> CreateForSingleEntity(string agentSignatureOptionsName, Id entityId, CancellationToken cancellationToken)
    {
        var transactionBuilder = await Create(agentSignatureOptionsName, cancellationToken);

        return new SingleEntityTransactionBuilder<TEntity>(transactionBuilder, entityId);
    }
}

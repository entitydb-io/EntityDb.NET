using EntityDb.Abstractions.Annotations;
using EntityDb.Abstractions.Leases;
using EntityDb.Abstractions.Queries;
using EntityDb.Abstractions.Tags;
using EntityDb.Abstractions.Transactions;
using EntityDb.Abstractions.Transactions.Steps;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Exceptions;
using EntityDb.SqlDb.Documents.AgentSignature;
using EntityDb.SqlDb.Documents.Command;
using EntityDb.SqlDb.Documents.Lease;
using EntityDb.SqlDb.Documents.Tag;
using EntityDb.SqlDb.Extensions;
using EntityDb.SqlDb.Sessions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Transactions;

internal class SqlDbTransactionRepository<TOptions> : DisposableResourceBaseClass, ITransactionRepository
    where TOptions : class
{
    private readonly IEnvelopeService<string> _envelopeService;
    private readonly ISqlDbSession<TOptions> _sqlDbSession;

    public SqlDbTransactionRepository
    (
        ISqlDbSession<TOptions> sqlDbSession,
        IEnvelopeService<string> envelopeService
    )
    {
        _sqlDbSession = sqlDbSession;
        _envelopeService = envelopeService;
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .EnumerateTransactionIds(_sqlDbSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .EnumerateTransactionIds(_sqlDbSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateTransactionIds(_sqlDbSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateTransactionIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateTransactionIds(_sqlDbSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .EnumerateEntitiesIds(_sqlDbSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .EnumerateEntityIds(_sqlDbSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateEntityIds(_sqlDbSession, cancellationToken);
    }

    public IAsyncEnumerable<Id> EnumerateEntityIds(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateEntityIds(_sqlDbSession, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .EnumerateData<AgentSignatureDocument, object, TOptions>(_sqlDbSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<object> EnumerateCommands(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .EnumerateData<CommandDocument, object, TOptions>(_sqlDbSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ILease> EnumerateLeases(ILeaseQuery leaseQuery,
        CancellationToken cancellationToken = default)
    {
        return LeaseDocument
            .GetQuery(leaseQuery)
            .EnumerateData<LeaseDocument, ILease, TOptions>(_sqlDbSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<ITag> EnumerateTags(ITagQuery tagQuery,
        CancellationToken cancellationToken = default)
    {
        return TagDocument
            .GetQuery(tagQuery)
            .EnumerateData<TagDocument, ITag, TOptions>(_sqlDbSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<IEntitiesAnnotation<object>> EnumerateAnnotatedAgentSignatures(IAgentSignatureQuery agentSignatureQuery,
        CancellationToken cancellationToken = default)
    {
        return AgentSignatureDocument
            .GetQuery(agentSignatureQuery)
            .EnumerateEntitiesAnnotation<AgentSignatureDocument, object, TOptions>(_sqlDbSession, _envelopeService, cancellationToken);
    }

    public IAsyncEnumerable<IEntityAnnotation<object>> EnumerateAnnotatedCommands(ICommandQuery commandQuery,
        CancellationToken cancellationToken = default)
    {
        return CommandDocument
            .GetQuery(commandQuery)
            .EnumerateEntityAnnotation<CommandDocument, object, TOptions>(_sqlDbSession, _envelopeService, cancellationToken);
    }

    public async Task<bool> PutTransaction(ITransaction transaction, CancellationToken cancellationToken = default)
    {
        try
        {
            await _sqlDbSession.StartTransaction(cancellationToken);

            await PutAgentSignature(transaction, cancellationToken);

            foreach (var transactionStep in transaction.Steps)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await (transactionStep switch
                {
                    IAppendCommandTransactionStep appendCommandTransactionStep
                        => PutCommand(transaction, appendCommandTransactionStep, cancellationToken),
                    IAddLeasesTransactionStep addLeasesTransactionStep
                        => PutLeases(transaction, addLeasesTransactionStep, cancellationToken),
                    IAddTagsTransactionStep addTagsTransactionStep
                        => PutTags(transaction, addTagsTransactionStep, cancellationToken),
                    IDeleteLeasesTransactionStep deleteLeasesTransactionStep
                        => DeleteLeases(deleteLeasesTransactionStep, cancellationToken),
                    IDeleteTagsTransactionStep deleteTagsTransactionStep
                        => DeleteTags(deleteTagsTransactionStep, cancellationToken),
                    _ => throw new NotSupportedException()
                });
            }

            await _sqlDbSession.CommitTransaction(cancellationToken);

            return true;
        }
        catch
        {
            await _sqlDbSession.AbortTransaction(cancellationToken);

            throw;
        }
    }

    public override async ValueTask DisposeAsync()
    {
        await _sqlDbSession.DisposeAsync();
    }

    private async Task PutAgentSignature(ITransaction transaction, CancellationToken cancellationToken)
    {
        await AgentSignatureDocument
            .GetInsert(_envelopeService, transaction)
            .Execute(_sqlDbSession, cancellationToken);
    }

    private async Task PutCommand(ITransaction transaction, IAppendCommandTransactionStep appendCommandTransactionStep,
        CancellationToken cancellationToken)
    {
        VersionZeroReservedException.ThrowIfZero(appendCommandTransactionStep.EntityVersionNumber);

        var previousVersionNumber = await CommandDocument
            .GetLastEntityVersionNumber(_sqlDbSession, appendCommandTransactionStep.EntityId, cancellationToken);

        OptimisticConcurrencyException.ThrowIfMismatch(previousVersionNumber,
            appendCommandTransactionStep.PreviousEntityVersionNumber);

        await CommandDocument
            .GetInsert(_envelopeService, transaction, appendCommandTransactionStep)
            .Execute(_sqlDbSession, cancellationToken);
    }

    private async Task PutLeases(ITransaction transaction, IAddLeasesTransactionStep addLeasesTransactionStep,
        CancellationToken cancellationToken)
    {
        await LeaseDocument
            .GetInsert(_envelopeService, transaction, addLeasesTransactionStep)
            .Execute(_sqlDbSession, cancellationToken);
    }

    private async Task PutTags(ITransaction transaction, IAddTagsTransactionStep addTagsTransactionStep,
        CancellationToken cancellationToken)
    {
        await TagDocument
            .GetInsert(_envelopeService, transaction, addTagsTransactionStep)
            .Execute(_sqlDbSession, cancellationToken);
    }

    private async Task DeleteLeases(IDeleteLeasesTransactionStep deleteLeasesTransactionStep,
        CancellationToken cancellationToken)
    {
        await LeaseDocument
            .GetDeleteCommand(deleteLeasesTransactionStep)
            .Execute(_sqlDbSession, cancellationToken);
    }

    private async Task DeleteTags(IDeleteTagsTransactionStep deleteTagsTransactionStep,
        CancellationToken cancellationToken)
    {
        await TagDocument
            .GetDeleteCommand(deleteTagsTransactionStep)
            .Execute(_sqlDbSession, cancellationToken);
    }
}

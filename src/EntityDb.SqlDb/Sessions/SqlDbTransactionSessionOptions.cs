using System;
using System.Data;
using EntityDb.Abstractions.Transactions;

namespace EntityDb.SqlDb.Sessions;

/// <summary>
///     Configuration options for the PostgreSql implementation of <see cref="ITransactionRepository"/>.
/// </summary>
public class SqlDbTransactionSessionOptions
{
    /// <summary>
    ///     A connection string that is compatible with <see cref="object"/>
    /// </summary>
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    ///     If <c>true</c>, indicates the agent only intends to execute queries.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    ///     If <c>true</c>, indicates the agent can tolerate replication lag for queries.
    /// </summary>
    public bool SecondaryPreferred { get; set; }

    public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.Snapshot;

    /// <summary>
    ///     Determines how long to wait before a command should be automatically aborted.
    /// </summary>
    public TimeSpan WriteTimeout { get; set; }

    /// <summary>
    ///     Determines how long to wait before a query should be automatically killed.
    /// </summary>
    public TimeSpan? ReadTimeout { get; set; }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{nameof(SqlDbTransactionSessionOptions)}";
    }
}

using System;

namespace EntityDb.MongoDb.Sessions;

/// <summary>
/// 
/// </summary>
public class MongoTransactionSessionOptions
{
    public string ConnectionString { get; set; } = default!;

    public string Database { get; set; } = default!;

    /// <summary>
    ///     If <c>true</c>, indicates the agent only intends to execute queries.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    ///     If <c>true</c>, indicates the agent can tolerate replication lag for queries.
    /// </summary>
    public bool SecondaryPreferred { get; set; }

    /// <summary>
    ///     Determines how long to wait before a command should be automatically aborted.
    /// </summary>
    public TimeSpan? WriteTimeout { get; set; }

    /// <summary>
    ///     Determines how long to wait before a query should be automatically killed.
    /// </summary>
    public TimeSpan? ReadTimeout { get; set; }
}

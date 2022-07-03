namespace EntityDb.Redis.Sessions;

/// <summary>
/// 
/// </summary>
public class RedisSnapshotSessionOptions<TSnapshot>
{
    public string ConnectionString { get; set; } = default!;

    public string KeyNamespace { get; set; } = default!;

    /// <summary>
    ///     If <c>true</c>, indicates the agent only intends to execute queries.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    ///     If <c>true</c>, indicates the agent can tolerate replication lag for queries.
    /// </summary>
    public bool SecondaryPreferred { get; set; }
}

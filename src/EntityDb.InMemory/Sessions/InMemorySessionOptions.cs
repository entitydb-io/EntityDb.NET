namespace EntityDb.InMemory.Sessions;

/// <summary>
///     Configures the in-memory snapshot repository.
/// </summary>
public class InMemorySnapshotSessionOptions
{
    /// <summary>
    ///     If <c>true</c>, indicates the agent only intends to execute queries.
    /// </summary>
    public bool ReadOnly { get; set; }
}

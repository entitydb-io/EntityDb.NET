using EntityDb.Abstractions.Loggers;

namespace EntityDb.Common.Snapshots
{
    /// <summary>
    ///     Represents the agent's use case for the snapshot repository.
    /// </summary>
    public sealed record SnapshotSessionOptions
    {
        /// <summary>
        ///     If <c>true</c>, indicates the agent only intends to execute queries.
        /// </summary>
        public bool ReadOnly { get; set; }

        /// <summary>
        ///     If <c>true</c>, indicates the agent can tolerate replication lag for queries.
        /// </summary>
        public bool SecondaryPreferred { get; set; }
    }
}

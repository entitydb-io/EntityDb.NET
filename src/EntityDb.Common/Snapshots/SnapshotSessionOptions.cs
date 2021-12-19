using EntityDb.Abstractions.Loggers;

namespace EntityDb.Common.Snapshots
{
    /// <summary>
    ///     Represents the agent's use case for the snapshot repository.
    /// </summary>
    public sealed record SnapshotSessionOptions
    {
        /// <summary>
        ///     If <c>true</c>, indicates that all snapshots should be discarded when the repository is disposed.
        /// </summary>
        public bool TestMode { get; set; }

        /// <summary>
        ///     Overrides the logger for the session.
        /// </summary>
        public ILogger? LoggerOverride { get; set; }
    }
}

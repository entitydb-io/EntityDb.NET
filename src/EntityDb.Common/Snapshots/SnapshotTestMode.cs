namespace EntityDb.Common.Snapshots
{
    /// <summary>
    ///     Modifies the behavior of a repository to accomodate tests.
    /// </summary>
    public enum SnapshotTestMode
    {
        /// <summary>
        ///     Snapshots will be deleted after all snapshot repositories have been disposed.
        /// </summary>
        /// <remarks>
        ///     This is the preferred option when a test is using a service provider that is shared with other tests.
        /// </remarks>
        AllRepositoriesDisposed,

        /// <summary>
        ///     Snapshots will be deleted after the snapshot repository factory has been disposed.
        /// </summary>
        /// <remarks>
        ///     This is the preferred option when a test is creating a service provider that will only be used by that test.
        /// </remarks>
        RepositoryFactoryDisposed,
    }
}

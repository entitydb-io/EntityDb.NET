using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityDb.EntityFramework.Snapshots;

/// <inheritdoc />
public class SnapshotReferenceTypeConfiguration<TSnapshot> : IEntityTypeConfiguration<SnapshotReference<TSnapshot>>
    where TSnapshot : class
{
    private readonly string _snapshotReferencesTableName;
    private readonly string _snapshotsTableName;

    /// <summary>
    ///     Configure the napshot Reference Type.
    /// </summary>
    /// <param name="snapshotReferencesTableName">The name of the table for snapshot references.</param>
    /// <param name="snapshotsTableName">The name of the table for snapshots.</param>
    public SnapshotReferenceTypeConfiguration(string snapshotReferencesTableName, string snapshotsTableName)
    {
        _snapshotReferencesTableName = snapshotReferencesTableName;
        _snapshotsTableName = snapshotsTableName;
    }

    /// <inheritdoc />
    public virtual void Configure(EntityTypeBuilder<SnapshotReference<TSnapshot>> snapshotReferenceBuilder)
    {
        snapshotReferenceBuilder.ToTable(_snapshotReferencesTableName);

        snapshotReferenceBuilder
            .HasKey
            (
                nameof(SnapshotReference<TSnapshot>.Id)
            );

        snapshotReferenceBuilder
            .HasAlternateKey
            (
                nameof(SnapshotReference<TSnapshot>.PointerId),
                nameof(SnapshotReference<TSnapshot>.PointerVersionNumber)
            );

        var snapshotBuilder = snapshotReferenceBuilder
            .OwnsOne(snapshotReference => snapshotReference.Snapshot);

        Configure(snapshotBuilder);
    }

    /// <summary>
    ///     Configures the snapshot of type <typeparamref name="TSnapshot" />.
    /// </summary>
    /// <param name="snapshotBuilder">The builder to be used to configure the snapshot type.</param>
    protected virtual void Configure(OwnedNavigationBuilder<SnapshotReference<TSnapshot>, TSnapshot> snapshotBuilder)
    {
        snapshotBuilder.ToTable(_snapshotsTableName);
    }
}

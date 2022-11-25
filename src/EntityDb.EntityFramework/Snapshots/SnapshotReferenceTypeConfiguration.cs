using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityDb.EntityFramework.Snapshots;

/// <inheritdoc />
public class SnapshotReferenceTypeConfiguration<TSnapshot> : IEntityTypeConfiguration<SnapshotReference<TSnapshot>>
    where TSnapshot : class
{
    /// <inheritdoc />
    public virtual void Configure(EntityTypeBuilder<SnapshotReference<TSnapshot>> snapshotReferenceBuilder)
    {
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
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityDb.EntityFramework.Snapshots;

/// <inheritdoc />
public class SnapshotReferenceTypeConfiguration<TSnapshot> : IEntityTypeConfiguration<SnapshotReference<TSnapshot>>
    where TSnapshot : class
{
    private readonly string _tableName = $"{typeof(TSnapshot).Name}SnapshotReferences";

    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<SnapshotReference<TSnapshot>> snapshotReferenceBuilder)
    {
        snapshotReferenceBuilder
            .ToTable(_tableName);

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

        snapshotReferenceBuilder
            .Navigation(snapshotReference => snapshotReference.Snapshot)
            .AutoInclude();

        snapshotReferenceBuilder
            .HasOne(snapshotReference => snapshotReference.Snapshot);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EntityDb.Common.Tests.Implementations.Snapshots;

public class SnapshotTypeConfiguration<TSnapshot> : IEntityTypeConfiguration<TSnapshot>
    where TSnapshot : class, ISnapshotWithTestLogic<TSnapshot>
{
    private readonly string _tableName = $"{typeof(TSnapshot).Name}s";

    public void Configure(EntityTypeBuilder<TSnapshot> snapshotBuilder)
    {
        snapshotBuilder
            .ToTable(_tableName);

        snapshotBuilder
            .HasKey
            (
                nameof(ISnapshotWithTestLogic<TSnapshot>.IdValue),
                nameof(ISnapshotWithTestLogic<TSnapshot>.VersionNumberValue)
            );
    }
}

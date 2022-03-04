using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace EntityDb.Common.Extensions;

internal static class SnapshotRepositoryExtensions
{
    public static ISnapshotRepository<TSnapshot> UseTryCatch<TSnapshot>
    (
        this ISnapshotRepository<TSnapshot> snapshotRepository,
        ILogger logger
    )
    {
        return new TryCatchSnapshotRepository<TSnapshot>(snapshotRepository, logger);
    }

    public static async Task<TSnapshot?> GetSnapshotOrDefault<TSnapshot>(this ISnapshotRepository<TSnapshot>? snapshotRepository, Id snapshotId)
    {
        if (snapshotRepository != null)
        {
            return await snapshotRepository.GetSnapshot(snapshotId);
        }

        return default;
    }
}

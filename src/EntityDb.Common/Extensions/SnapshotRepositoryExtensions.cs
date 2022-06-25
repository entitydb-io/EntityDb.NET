using EntityDb.Abstractions.Snapshots;
using EntityDb.Abstractions.ValueObjects;
using System.Threading.Tasks;

namespace EntityDb.Common.Extensions;

internal static class SnapshotRepositoryExtensions
{
    public static async Task<TSnapshot?> GetSnapshotOrDefault<TSnapshot>(this ISnapshotRepository<TSnapshot>? snapshotRepository, Id snapshotId)
    {
        if (snapshotRepository is not null)
        {
            return await snapshotRepository.GetSnapshot(snapshotId);
        }

        return default;
    }
}

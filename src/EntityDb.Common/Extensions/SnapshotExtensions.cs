using EntityDb.Abstractions.ValueObjects;
using EntityDb.Common.Snapshots;

namespace EntityDb.Common.Extensions;

internal static class SnapshotExtensions
{
    public static Pointer GetPointer<TSnapshot>(this TSnapshot snapshot)
        where TSnapshot : ISnapshot<TSnapshot>
    {
        return snapshot.GetId() + snapshot.GetVersionNumber();
    }
}

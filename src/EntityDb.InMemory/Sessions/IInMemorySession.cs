using EntityDb.Abstractions.ValueObjects;
using System.Threading.Tasks;

namespace EntityDb.InMemory.Sessions;

internal interface IInMemorySession<TSnapshot>
{
    Task<bool> Insert(Id snapshotId, TSnapshot snapshot);

    Task<TSnapshot?> Get(Id snapshotId);

    Task<bool> Delete(Id[] snapshotIds);
}

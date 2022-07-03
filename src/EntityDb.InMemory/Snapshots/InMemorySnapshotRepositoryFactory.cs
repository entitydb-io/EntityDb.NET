using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Snapshots;
using EntityDb.InMemory.Sessions;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.InMemory.Snapshots;

internal class InMemorySnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass,
    ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly IInMemorySession<TSnapshot> _inMemorySession;
    private readonly IOptionsFactory<InMemorySnapshotSessionOptions> _optionsFactory;
    private readonly IServiceProvider _serviceProvider;

    public InMemorySnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IInMemorySession<TSnapshot> inMemorySession,
        IOptionsFactory<InMemorySnapshotSessionOptions> optionsFactory
    )
    {
        _serviceProvider = serviceProvider;
        _inMemorySession = inMemorySession;
        _optionsFactory = optionsFactory;
    }

    public async Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        var options = _optionsFactory.Create(snapshotSessionOptionsName);

        var inMemorySession = options.ReadOnly
            ? new ReadOnlyInMemorySession<TSnapshot>(_inMemorySession)
            : _inMemorySession;

        var inMemorySnapshotRepository = new InMemorySnapshotRepository<TSnapshot>(inMemorySession);

        cancellationToken.ThrowIfCancellationRequested();

        return TryCatchSnapshotRepository<TSnapshot>.Create(_serviceProvider, inMemorySnapshotRepository);
    }
}

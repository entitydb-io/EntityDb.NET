using EntityDb.Abstractions.Snapshots;
using EntityDb.Common.Disposables;
using EntityDb.Common.Envelopes;
using EntityDb.Common.Extensions;
using EntityDb.Common.Snapshots;
using EntityDb.Common.TypeResolvers;
using EntityDb.InMemory.Sessions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace EntityDb.InMemory.Snapshots;

internal class InMemorySnapshotRepositoryFactory<TSnapshot> : DisposableResourceBaseClass, ISnapshotRepositoryFactory<TSnapshot>
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IInMemorySession<TSnapshot> _inMemorySession;
    private readonly IOptionsFactory<SnapshotSessionOptions> _optionsFactory;

    public InMemorySnapshotRepositoryFactory
    (
        IServiceProvider serviceProvider,
        IInMemorySession<TSnapshot> inMemorySession,
        IOptionsFactory<SnapshotSessionOptions> optionsFactory
    )
    {
        _serviceProvider = serviceProvider;
        _inMemorySession = inMemorySession;
        _optionsFactory = optionsFactory;
    }

    public async Task<ISnapshotRepository<TSnapshot>> CreateRepository(string snapshotSessionOptionsName)
    {
        await Task.Yield();
        
        var snapshotSessionOptions = _optionsFactory.Create(snapshotSessionOptionsName);

        var inMemorySession = snapshotSessionOptions.ReadOnly
            ? new ReadOnlyInMemorySession<TSnapshot>(_inMemorySession)
            : _inMemorySession;

        var inMemorySnapshotRepository = new InMemorySnapshotRepository<TSnapshot>(inMemorySession);

        return TryCatchSnapshotRepository<TSnapshot>.Create(_serviceProvider, inMemorySnapshotRepository);
    }
}

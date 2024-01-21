using EntityDb.Abstractions.Sources;
using Microsoft.Extensions.DependencyInjection;

namespace EntityDb.Common.Sources;

internal sealed class PublishSourceRepository : SourceRepositoryWrapper
{
    private readonly IEnumerable<ISourceSubscriber> _sourceSubscribers;

    public PublishSourceRepository
    (
        ISourceRepository sourceRepository,
        IEnumerable<ISourceSubscriber> sourceSubscribers
    ) : base(sourceRepository)
    {
        _sourceSubscribers = sourceSubscribers;
    }

    public override async Task<bool> Commit(Source source, CancellationToken cancellationToken = default)
    {
        var committed = await base.Commit(source, cancellationToken);

        if (!committed)
        {
            return false;
        }

        foreach (var sourceSubscriber in _sourceSubscribers)
        {
            sourceSubscriber.Notify(source);
        }

        return true;
    }

    public static ISourceRepository Create(IServiceProvider serviceProvider,
        ISourceRepository sourceRepository)
    {
        return ActivatorUtilities.CreateInstance<PublishSourceRepository>(serviceProvider,
            sourceRepository);
    }
}

using EntityDb.Common.Envelopes;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Aws.Sources.Processors.Queues;

[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
internal record SqsSourceProcessorMessage
{
    public required Guid StateId { get; init; }
    public required Guid SourceId { get; init; }
    public required string SourceRepositoryOptionsName { get; init; }
    public required EnvelopeHeaders SourceProcessorEnvelopeHeaders { get; init; }
}

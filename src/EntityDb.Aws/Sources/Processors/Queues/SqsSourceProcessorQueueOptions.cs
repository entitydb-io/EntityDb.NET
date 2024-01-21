using Amazon.SQS;
using Microsoft.Extensions.Logging;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Aws.Sources.Processors.Queues;

/// <summary>
///     Options for configuring the SQS Source Processor Queues.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "Not used in tests.")]
public class SqsSourceProcessorQueueOptions
{
    /// <summary>
    ///     Retrieve the IAmazonSQS client as well as the queue url. Note that queues
    ///     **MUST** be first-in first-out (FIFO).
    /// </summary>
    public Func<IServiceProvider, Task<(IAmazonSQS, string)>> AmazonSqsFactory { get; set; } = _ => throw new NotImplementedException();
    
    /// <summary>
    ///     Map the delta to a source repository options name. Should use pattern matching, such as <c>is IReducer{TEntity}</c>
    ///     or <c>is IMutator{TProjection}</c>
    /// </summary>
    public Func<object, string> GetSourceRepositoryOptionsNameFromDelta { get; set; } = _ => throw new NotImplementedException();
    
    /// <summary>
    ///     Limits how fast sources are enqueued into SQS
    /// </summary>
    public TimeSpan EnqueueDelay { get; set; } = TimeSpan.FromSeconds(5);
    
    /// <summary>
    ///     Limits how fast sources are dequeued from SQS when the previous attempt to receive a source
    ///     yielded a source.
    /// </summary>
    public TimeSpan ActiveDequeueDelay { get; set; } = TimeSpan.FromSeconds(10);
    
    /// <summary>
    ///     Limits how fast sources are dequeued from SQS when the previous attempt to receive a source
    ///     yielded nothing.
    /// </summary>
    public TimeSpan IdleDequeueDelay { get; set; } = TimeSpan.FromMinutes(1);
    
    /// <summary>
    ///     Determines the log level of logs that give debugging information.
    /// </summary>
    public LogLevel DebugLogLevel { get; set; } = LogLevel.Debug;
}

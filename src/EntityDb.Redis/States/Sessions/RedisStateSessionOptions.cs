using EntityDb.Abstractions.States;
using StackExchange.Redis;
using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Redis.States.Sessions;

/// <summary>
///     Configuration options for the Redis implementation of <see cref="IStateRepository{TState}" />.
/// </summary>
public sealed class RedisStateSessionOptions
{
    /// <summary>
    ///     A connection string that is compatible with <see cref="ConfigurationOptions.Parse(string)" />
    /// </summary>
    public string ConnectionString { get; set; } = default!;

    /// <summary>
    ///     Choose a key namespace for states. States are stored with keys in the following format:
    ///     <c>{KeyNamespace}#{StateId}@{StateVersion}</c>
    /// </summary>
    public string KeyNamespace { get; set; } = default!;

    /// <summary>
    ///     If <c>true</c>, indicates the agent only intends to execute queries.
    /// </summary>
    public bool ReadOnly { get; set; }

    /// <summary>
    ///     If <c>true</c>, indicates the agent can tolerate replication lag for queries.
    /// </summary>
    public bool SecondaryPreferred { get; set; }

    /// <inheritdoc />
    [ExcludeFromCodeCoverage(Justification = "This is only overridden to make test names better.")]
    public override string ToString()
    {
        return $"{nameof(RedisStateSessionOptions)}";
    }
}

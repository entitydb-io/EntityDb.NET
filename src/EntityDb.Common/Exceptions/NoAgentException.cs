using EntityDb.Abstractions.Sources.Agents;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when the <see cref="IAgentAccessor" /> cannot return an instance of
///     <see cref="IAgent" />.
/// </summary>
public sealed class NoAgentException : Exception;

using EntityDb.Common.TypeResolvers;
using System;
using System.Collections.Generic;

namespace EntityDb.Common.Envelopes;

/// <summary>
///     Represents information that can be used to resolve a <see cref="Type" /> by using a <see cref="ITypeResolver" />.
/// </summary>
/// <param name="Value">The backing value.</param>
public readonly record struct EnvelopeHeaders(IReadOnlyDictionary<string, string> Value);

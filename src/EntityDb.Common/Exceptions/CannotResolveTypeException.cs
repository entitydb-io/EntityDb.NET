using EntityDb.Common.TypeResolvers;

namespace EntityDb.Common.Exceptions;

/// <summary>
///     The exception that is thrown when a <see cref="ITypeResolver" /> cannot resolve a type.
/// </summary>
public sealed class CannotResolveTypeException : Exception
{
}

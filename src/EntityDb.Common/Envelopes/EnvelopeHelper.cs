using System.Diagnostics.CodeAnalysis;

namespace EntityDb.Common.Envelopes;

internal static class EnvelopeHelper
{
    public const string Platform = nameof(Platform);
    public const string ThisPlatform = ".NET";
    public const string Type = nameof(Type);
    public const string AssemblyFullName = nameof(AssemblyFullName);
    public const string TypeFullName = nameof(TypeFullName);
    public const string MemberInfoName = nameof(MemberInfoName);

    public static bool NotThisPlatform(EnvelopeHeaders envelopeHeaders)
    {
        return !envelopeHeaders.Value.TryGetValue(Platform, out var platform) || platform != ThisPlatform;
    }

    public static bool TryGetAssemblyFullName
    (
        EnvelopeHeaders envelopeHeaders,
        [NotNullWhen(true)] out string? assemblyFullName
    )
    {
        return envelopeHeaders.Value.TryGetValue(AssemblyFullName, out assemblyFullName);
    }

    public static bool TryGetTypeFullName
    (
        EnvelopeHeaders envelopeHeaders,
        [NotNullWhen(true)] out string? typeFullName
    )
    {
        return envelopeHeaders.Value.TryGetValue(TypeFullName, out typeFullName);
    }

    public static bool TryGetMemberInfoName
    (
        EnvelopeHeaders envelopeHeaders,
        [NotNullWhen(true)] out string? memberInfoName
    )
    {
        return envelopeHeaders.Value.TryGetValue(MemberInfoName, out memberInfoName);
    }

    public static EnvelopeHeaders GetEnvelopeHeaders
    (
        Type type,
        bool includeFullNames = true,
        bool includeMemberInfoName = true
    )
    {
        var value = new Dictionary<string, string> { [Platform] = ThisPlatform, [Type] = type.Name };

        if (includeFullNames)
        {
            var assemblyFullName = type.Assembly.FullName;

            if (assemblyFullName is not null)
            {
                value.Add(AssemblyFullName, assemblyFullName);
            }

            var typeFullName = type.FullName;

            if (typeFullName is not null)
            {
                value.Add(TypeFullName, typeFullName);
            }
        }

        if (includeMemberInfoName)
        {
            value.Add(MemberInfoName, type.Name);
        }

        return new EnvelopeHeaders(value);
    }

    public static IEnumerable<string> GetTypeHeaderValues(this IEnumerable<Type> types)
    {
        return types.Select(type => type.Name);
    }
}

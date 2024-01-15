using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Common.Tests.Implementations.Sources.Attributes;

public sealed record CountLease(ulong Number) : ILease
{
    public string Scope => "";
    public string Label => "";
    public string Value => $"{Number}";
}

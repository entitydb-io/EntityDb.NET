using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Common.Tests.Implementations.States.Attributes;

public record CountLease(ulong Number) : ILease
{
    public string Scope => "";
    public string Label => "";
    public string Value => $"{Number}";
}

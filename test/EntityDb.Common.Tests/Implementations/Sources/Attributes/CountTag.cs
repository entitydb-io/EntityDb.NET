using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Common.Tests.Implementations.Sources.Attributes;

public sealed record CountTag(ulong Number) : ITag
{
    public string Label => "";
    public string Value => $"{Number}";
}

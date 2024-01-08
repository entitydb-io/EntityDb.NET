using EntityDb.Abstractions.Sources.Attributes;

namespace EntityDb.Common.Tests.Implementations.Tags;

public record CountTag(ulong Number) : ITag
{
    public string Label => "";
    public string Value => $"{Number}";
}
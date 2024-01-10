using EntityDb.Abstractions.States.Attributes;

namespace EntityDb.Common.Tests.Implementations.States.Attributes;

public record CountTag(ulong Number) : ITag
{
    public string Label => "";
    public string Value => $"{Number}";
}

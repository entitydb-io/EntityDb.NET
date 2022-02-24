using EntityDb.Abstractions.Tags;

namespace EntityDb.Common.Tests.Implementations.Tags;

public record CountTag(int Number) : ITag
{
    public string Label => $"{Number}";
    public string Value => "";
}
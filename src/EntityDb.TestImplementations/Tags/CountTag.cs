using EntityDb.Abstractions.Tags;

namespace EntityDb.TestImplementations.Tags
{
    public record CountTag(int Number) : ITag
    {
        public string Label => $"{Number}";
        public string Value => "";
    }
}

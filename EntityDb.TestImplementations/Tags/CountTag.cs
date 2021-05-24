using EntityDb.Abstractions.Tags;

namespace EntityDb.TestImplementations.Tags
{
    public record CountTag(int Number) : ITag
    {
        public string Scope => $"{Number}";
        public string Label => "";
        public string Value => "";
    }
}

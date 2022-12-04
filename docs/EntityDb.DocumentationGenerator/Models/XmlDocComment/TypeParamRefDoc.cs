using System.Xml.Serialization;
using System.Xml;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class TypeParamRefDoc
{
    [XmlAttribute("name")]
    public required string Name { get; init; }
}

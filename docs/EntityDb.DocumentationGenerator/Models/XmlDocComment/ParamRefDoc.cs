using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class ParamRefDoc
{
    [XmlAttribute("name")]
    public required string Name { get; init; }
}

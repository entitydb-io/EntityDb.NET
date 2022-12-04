using System.Xml.Serialization;
using System.Xml;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class ParamRefDoc
{
    [XmlAttribute("name")]
    public required string Name { get; init; }
}

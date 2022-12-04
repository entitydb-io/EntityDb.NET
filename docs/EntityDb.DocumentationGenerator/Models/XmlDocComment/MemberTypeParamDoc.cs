using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberTypeParamDoc
{
    [XmlText]
    public required string Text { get; init; }

    [XmlAttribute("name")]
    public required string Name { get; init; }
}

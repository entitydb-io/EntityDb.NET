using System.Xml.Serialization;
using System.Xml;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberParamDoc
{
    [XmlText]
    public required string Text { get; init; }

    [XmlAttribute("name")]
    public required string Name { get; init; }
}

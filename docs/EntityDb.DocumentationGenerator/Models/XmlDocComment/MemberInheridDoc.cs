using System.Xml.Serialization;
using System.Xml;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberInheridDoc
{
    [XmlAttribute("cref")]
    public required string SeeRef { get; init; }
}

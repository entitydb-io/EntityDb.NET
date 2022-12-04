using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberInheritDoc
{
    [XmlAttribute("cref")]
    public required string SeeRef { get; init; }
}

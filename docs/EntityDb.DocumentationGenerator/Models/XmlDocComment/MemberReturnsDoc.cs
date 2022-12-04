using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberReturnsDoc
{
    [XmlText]
    public required string Text { get; init; }
}

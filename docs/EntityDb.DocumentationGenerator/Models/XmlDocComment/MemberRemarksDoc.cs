using System.Xml.Serialization;
using System.Xml;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberRemarksDoc
{
    [XmlText]
    public required string Text { get; init; }
}

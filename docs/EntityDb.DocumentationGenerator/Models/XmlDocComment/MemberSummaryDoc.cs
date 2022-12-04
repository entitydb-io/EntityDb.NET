using System.Xml.Serialization;
using System.Xml;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberSummaryDoc
{
    [XmlText(typeof(XmlText))]
    [XmlAnyElement]
    public required XmlNode[] Text { get; init; }
}

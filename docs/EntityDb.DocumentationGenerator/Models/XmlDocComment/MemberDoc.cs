using System.Xml.Serialization;
using System.Xml;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberDoc
{
    [XmlElement("param", typeof(MemberParamDoc))]
    [XmlElement("remarks", typeof(MemberRemarksDoc))]
    [XmlElement("returns", typeof(MemberReturnsDoc))]
    [XmlElement("summary", typeof(MemberSummaryDoc))]
    [XmlElement("typeparam", typeof(MemberTypeParamDoc))]
    [XmlElement("inheritdoc", typeof(MemberInheridDoc))]
    [XmlElement("ignore", typeof(MemberIgnoreDoc))]
    public required object[] Items { get; init; }

    [XmlAttribute("name")]
    public required string Name { get; init; }
}

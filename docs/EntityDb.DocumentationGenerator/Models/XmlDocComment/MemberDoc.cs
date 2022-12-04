using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class MemberDoc
{
    [XmlElement("param", typeof(ParamDoc))]
    [XmlElement("remarks", typeof(RemarksDoc))]
    [XmlElement("returns", typeof(ReturnsDoc))]
    [XmlElement("summary", typeof(SummaryDoc))]
    [XmlElement("typeparam", typeof(TypeParamDoc))]
    [XmlElement("inheritdoc", typeof(InheritDoc))]
    [XmlElement("ignore", typeof(IgnoreDoc))]
    public required object[] Items { get; init; }

    [XmlAttribute("name")]
    public required string Name { get; init; }
}

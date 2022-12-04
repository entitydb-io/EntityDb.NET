using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

[XmlRoot("doc")]
public class DocFile
{
    [XmlElement("assembly")]
    public required AssemblyDoc Assembly { get; init; }

    [XmlArray("members")]
    [XmlArrayItem("member")]
    public required MemberDoc[] Members { get; init; }
}

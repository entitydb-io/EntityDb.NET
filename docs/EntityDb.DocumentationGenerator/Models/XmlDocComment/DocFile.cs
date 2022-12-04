using System.Xml.Serialization;
using System.Xml;

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

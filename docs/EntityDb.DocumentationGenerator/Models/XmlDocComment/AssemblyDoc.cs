using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class AssemblyDoc
{
    [XmlElement("name")]
    public required string Name { get; init; }
}

using System.Xml;
using System.Xml.Serialization;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

public class ParamRefDoc : DocWithMixedText
{
    [XmlAttribute("name")]
    public required string Name { get; init; }

    public override string DefaultText()
    {
        return Name;
    }
}

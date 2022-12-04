using System.Xml;
using System.Xml.Serialization;
using EntityDb.DocumentationGenerator.Models.Nodes;

namespace EntityDb.DocumentationGenerator.Models.XmlDocComment;

[XmlRoot("doc")]
public class DocFile
{
    [XmlElement("assembly")]
    public required AssemblyDoc Assembly { get; init; }

    [XmlArray("members")]
    [XmlArrayItem("member")]
    public required MemberDoc[] Members { get; init; }

    public void LoadInto(IDictionary<string, Node> xmlDocCommentMemberDictionary)
    {
        foreach (var member in Members)
        {
            var name = member.Name;

            if (xmlDocCommentMemberDictionary.TryGetValue(name, out var node))
            {
                foreach (var item in member.Items)
                {
                    node.AddDocumentation(item);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }
    }
}

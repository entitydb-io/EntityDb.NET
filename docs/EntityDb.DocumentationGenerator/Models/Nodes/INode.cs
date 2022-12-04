using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public interface INode
{
    MemberInheritDoc? InheritDoc { get; } //TODO: Do something with this?
}
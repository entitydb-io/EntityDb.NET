using System.Reflection;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Nodes;

public class ConstructorNode : MemberInfoNode
{
    public ConstructorInfo ConstructorInfo { get; }

    public ConstructorNode(ConstructorInfo constructorInfo)
    {
        ConstructorInfo = constructorInfo;
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberIgnoreDoc:
                // Used to make the Warning go away for public constructors on public classes
                //TODO: Consider making these classes internal, provide another way to use them.
                break;

            default:
                base.AddDocumentation(docCommentMemberItem);
                break;
        }
    }
}
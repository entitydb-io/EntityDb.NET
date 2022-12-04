using System.Reflection;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Nodes;

public class MethodNode : MemberInfoNode
{
    public MethodInfo MethodInfo { get; }

    public MemberReturnsDoc? Returns { get; set; }

    public MethodNode(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberReturnsDoc returnsDoc:
                Returns = returnsDoc;
                break;

            default:
                base.AddDocumentation(docCommentMemberItem);
                break;
        }
    }
}
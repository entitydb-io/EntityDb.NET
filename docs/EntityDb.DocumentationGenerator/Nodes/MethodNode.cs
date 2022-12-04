using System.Reflection;
using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Nodes;

public class MethodNode : MemberInfoNode
{
    public MethodInfo MethodInfo { get; }

    public string? Returns { get; set; }

    public MethodNode(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
    }

    public override void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberReturnsDoc returns:
                Returns = returns.Text;
                break;

            default:
                base.AddDocumentation(docCommentMemberItem);
                break;
        }
    }
}
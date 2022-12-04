using System.Reflection;
using static EntityDb.DocumentationGenerator.Services.DocCommentService.DocCommentService;

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
            case DocCommentMemberReturns returns:
                Returns = returns.Text;
                break;

            default:
                base.AddDocumentation(docCommentMemberItem);
                break;
        }
    }
}
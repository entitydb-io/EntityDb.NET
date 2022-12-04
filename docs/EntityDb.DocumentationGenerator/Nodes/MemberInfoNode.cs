using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Nodes;

public abstract class MemberInfoNode : INode
{
    public Dictionary<string, string> TypeParams { get; init; } = new();
    public Dictionary<string, string> Params { get; init; } = new();
    public MemberSummaryDoc? Summary { get; set; }
    public MemberRemarksDoc? Remarks { get; set; }
    public string? InheritDoc { get; set; }

    public virtual void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberTypeParamDoc typeParam:
                TypeParams.Add(typeParam.Name, typeParam.Text);
                break;

            case MemberParamDoc param:
                TypeParams.Add(param.Name, param.Text);
                break;

            case MemberSummaryDoc summaryDoc:
                Summary = summaryDoc;
                break;

            case MemberRemarksDoc remarksDoc:
                Remarks = remarksDoc;
                break;

            case MemberInheridDoc inheritDoc:
                InheritDoc = inheritDoc.SeeRef;
                break;

            default:
                throw new NotImplementedException();
        }
    }
}
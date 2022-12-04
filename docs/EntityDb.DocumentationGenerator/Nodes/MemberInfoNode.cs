using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Nodes;

public abstract class MemberInfoNode : INode
{
    public Dictionary<string, MemberTypeParamDoc> TypeParams { get; init; } = new();
    public Dictionary<string, MemberParamDoc> Params { get; init; } = new();
    public MemberSummaryDoc? Summary { get; set; }
    public MemberRemarksDoc? Remarks { get; set; }
    public string? InheritDoc { get; set; }

    public virtual void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberTypeParamDoc typeParamDoc:
                TypeParams.Add(typeParamDoc.Name, typeParamDoc);
                break;

            case MemberParamDoc paramDoc:
                Params.Add(paramDoc.Name, paramDoc);
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
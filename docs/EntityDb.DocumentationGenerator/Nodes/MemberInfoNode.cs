using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Nodes;

public abstract class MemberInfoNode : INode
{
    public Dictionary<string, MemberTypeParamDoc> TypeParamDocs { get; init; } = new();
    public Dictionary<string, MemberParamDoc> ParamDocs { get; init; } = new();
    public MemberSummaryDoc? SummaryDoc { get; set; }
    public MemberRemarksDoc? RemarksDoc { get; set; }

    //TODO: Do something with this?
    public MemberInheritDoc? InheritDoc { get; set; }

    public virtual void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case MemberTypeParamDoc typeParamDoc:
                TypeParamDocs.Add(typeParamDoc.Name, typeParamDoc);
                break;

            case MemberParamDoc paramDoc:
                ParamDocs.Add(paramDoc.Name, paramDoc);
                break;

            case MemberSummaryDoc summaryDoc:
                SummaryDoc = summaryDoc;
                break;

            case MemberRemarksDoc remarksDoc:
                RemarksDoc = remarksDoc;
                break;

            case MemberInheritDoc inheritDoc:
                InheritDoc = inheritDoc;
                break;

            default:
                throw new NotImplementedException();
        }
    }
}
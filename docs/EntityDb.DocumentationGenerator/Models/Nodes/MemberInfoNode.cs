using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public abstract class MemberInfoNode : INode
{
    public MemberSummaryDoc? SummaryDoc { get; set; }
    public MemberRemarksDoc? RemarksDoc { get; set; }
    public MemberInheritDoc? InheritDoc { get; set; }

    public virtual void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
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
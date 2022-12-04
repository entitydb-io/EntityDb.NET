using EntityDb.DocumentationGenerator.Models.XmlDocComment;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public abstract class Node
{
    public SummaryDoc? SummaryDoc { get; set; }
    public RemarksDoc? RemarksDoc { get; set; }
    public InheritDoc? InheritDoc { get; set; }
    public bool Ignore { get; set; }

    public virtual void AddDocumentation(object docCommentMemberItem)
    {
        switch (docCommentMemberItem)
        {
            case SummaryDoc summaryDoc:
                SummaryDoc = summaryDoc;
                break;

            case RemarksDoc remarksDoc:
                RemarksDoc = remarksDoc;
                break;

            case InheritDoc inheritDoc:
                InheritDoc = inheritDoc;
                break;

            case IgnoreDoc:
                Ignore = true;
                break;

            default:
                throw new NotImplementedException();
        }
    }
}
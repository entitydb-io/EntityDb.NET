using System.Reflection;
using System.Xml.XPath;
using EntityDb.DocumentationGenerator.Helpers;

namespace EntityDb.DocumentationGenerator.Nodes;

public class MethodNode : MemberInfoNode
{
    private readonly MethodInfo methodInfo;

    public string? Returns { get; set; }

    public MethodNode(MethodInfo methodInfo) : base(methodInfo)
    {
        this.methodInfo = methodInfo;
    }

    public override string GetXmlDocCommentName()
    {
        return MethodInfoHelper.GetXmlDocCommentName(methodInfo);
    }

    public override void AddDocumentation(XPathNavigator documentation)
    {
        switch (documentation.Name)
        {
            case "returns":
                Returns = documentation.InnerXml.Trim();
                break;

            default:
                base.AddDocumentation(documentation);
                break;
        }
    }
}
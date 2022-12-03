using System.Reflection;
using System.Xml.XPath;
using EntityDb.DocumentationGenerator.Helpers;

namespace EntityDb.DocumentationGenerator.Nodes;

public abstract class MemberInfoNode : Node
{
    private readonly MemberInfo memberInfo;
    public Dictionary<string, string> TypeParams { get; init; } = new();
    public Dictionary<string, string> Params { get; init; } = new();

    protected MemberInfoNode(MemberInfo memberInfo)
    {
        this.memberInfo = memberInfo;
    }

    public virtual string GetXmlDocCommentName()
    {
        return MemberInfoHelper.GetXmlDocCommentName(memberInfo);
    }

    public override void AddDocumentation(XPathNavigator documentation)
    {
        switch (documentation.Name)
        {
            case "typeparam":
                var typeParamName = documentation.GetAttribute("name", "");
                var typeParamDesc = documentation.InnerXml.Trim();

                TypeParams.Add(typeParamName, typeParamDesc);
                break;

            case "param":
                var paramName = documentation.GetAttribute("name", "");
                var paramDesc = documentation.InnerXml.Trim();

                Params.Add(paramName, paramDesc);
                break;

            default:
                base.AddDocumentation(documentation);
                break;
        }
    }
}
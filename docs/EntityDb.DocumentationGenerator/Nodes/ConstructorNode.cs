using System.Reflection;
using System.Xml.XPath;
using EntityDb.DocumentationGenerator.Helpers;

namespace EntityDb.DocumentationGenerator.Nodes;

public class ConstructorNode : MemberInfoNode
{
    private readonly ConstructorInfo constructorInfo;

    public ConstructorNode(ConstructorInfo constructorInfo) : base(constructorInfo)
    {
        this.constructorInfo = constructorInfo;
    }

    public override string GetXmlDocCommentName()
    {
        return ConstructorInfoHelper.GetXmlDocCommentName(constructorInfo);
    }

    public override void AddDocumentation(XPathNavigator documentation)
    {
        switch (documentation.Name)
        {
            case "ignore":
                // Don't know what to put for doc comment on ctors
                break;

            default:
                base.AddDocumentation(documentation);
                break;
        }
    }
}
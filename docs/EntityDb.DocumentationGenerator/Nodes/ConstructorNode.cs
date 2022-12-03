using System.Reflection;
using System.Xml.XPath;
using EntityDb.DocumentationGenerator.Helpers;

namespace EntityDb.DocumentationGenerator.Nodes;

public class ConstructorNode : MemberInfoNode
{
    public ConstructorInfo ConstructorInfo { get; }

    public ConstructorNode(ConstructorInfo constructorInfo) : base(constructorInfo)
    {
        ConstructorInfo = constructorInfo;
    }

    public override string GetXmlDocCommentName()
    {
        return ConstructorInfoHelper.GetXmlDocCommentName(ConstructorInfo);
    }

    public override void AddDocumentation(XPathNavigator documentation)
    {
        switch (documentation.Name)
        {
            case "ignore":
                // Used to make the Warning go away for public constructors on public classes
                //TODO: Consider making these classes internal, provide another way to use them.
                break;

            default:
                base.AddDocumentation(documentation);
                break;
        }
    }
}
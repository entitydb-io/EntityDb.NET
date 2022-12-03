using System.Reflection;
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
}
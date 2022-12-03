using System.Reflection;
using EntityDb.DocumentationGenerator.Helpers;

namespace EntityDb.DocumentationGenerator.Nodes;

public abstract class MemberInfoNode : Node
{
    private readonly MemberInfo memberInfo;

    protected MemberInfoNode(MemberInfo memberInfo)
    {
        this.memberInfo = memberInfo;
    }

    public virtual string GetXmlDocCommentName()
    {
        return MemberInfoHelper.GetXmlDocCommentName(memberInfo);
    }

    public override void Add(string path, Node node)
    {
        throw new InvalidOperationException();
    }
}
using System.Reflection;
using EntityDb.DocumentationGenerator.Helpers;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class MethodNode : MemberInfoNode
    {
        private readonly MethodInfo methodInfo;

        public MethodNode(MethodInfo methodInfo) : base(methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public override string GetXmlDocCommentName()
        {
            return MethodInfoHelper.GetXmlDocCommentName(methodInfo);
        }
    }
}
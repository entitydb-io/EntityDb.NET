using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class MethodNode : MemberInfoNode
    {
        private readonly MethodInfo methodInfo;

        public MethodNode(MethodInfo methodInfo) : base(methodInfo)
        {
            this.methodInfo = methodInfo;
        }

        public override string GetName()
        {
            return base.GetName();
        }
    }
}
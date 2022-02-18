using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public abstract class MemberInfoNode : Node
    {
        private readonly MemberInfo memberInfo;

        protected MemberInfoNode(MemberInfo memberInfo)
        {
            this.memberInfo = memberInfo;
        }

        public virtual string GetName()
        {
            return MemberInfoNameHelper.ReplacePeriodWithHash(memberInfo.Name);
        }
    }
}
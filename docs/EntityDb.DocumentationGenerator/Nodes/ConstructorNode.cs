using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class ConstructorNode : MemberInfoNode
    {
        private readonly ConstructorInfo constructorInfo;

        public ConstructorNode(ConstructorInfo constructorInfo) : base(constructorInfo)
        {
            this.constructorInfo = constructorInfo;
        }
    }
}
using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class TypeInfoNode : MemberInfoNode
    {
        private readonly TypeInfo typeInfo;

        public TypeInfoNode(TypeInfo typeInfo) : base(typeInfo)
        {
            this.typeInfo = typeInfo;
        }
    }
}
using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class FieldNode : MemberInfoNode
    {
        private readonly FieldInfo fieldInfo;

        public FieldNode(FieldInfo fieldInfo) : base(fieldInfo)
        {
            this.fieldInfo = fieldInfo;
        }
    }
}
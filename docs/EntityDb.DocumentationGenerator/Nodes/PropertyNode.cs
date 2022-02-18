using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class PropertyNode : MemberInfoNode
    {
        private readonly PropertyInfo propertyInfo;

        public PropertyNode(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            this.propertyInfo = propertyInfo;
        }

        public override string GetName()
        {
            return base.GetName();
        }
    }
}
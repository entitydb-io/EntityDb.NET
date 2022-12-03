using System.Reflection;
using EntityDb.DocumentationGenerator.Helpers;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class PropertyNode : MemberInfoNode
    {
        private readonly PropertyInfo propertyInfo;

        public PropertyNode(PropertyInfo propertyInfo) : base(propertyInfo)
        {
            this.propertyInfo = propertyInfo;
        }

        public override string GetXmlDocCommentName()
        {
            return PropertyInfoHelper.GetXmlDocCommentName(propertyInfo);
        }
    }
}
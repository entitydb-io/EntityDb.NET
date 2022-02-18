using System;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class TypeNode : Node
    {
        private readonly Type type;

        public TypeNode(Type type)
        {
            this.type = type;
        }
    }
}
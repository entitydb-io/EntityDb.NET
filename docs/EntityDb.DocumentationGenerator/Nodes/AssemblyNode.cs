using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes
{
    public class AssemblyNode : Node
    {
        private readonly Assembly assembly;

        public AssemblyNode(Assembly assembly)
        {
            this.assembly = assembly;
        }
    }
}
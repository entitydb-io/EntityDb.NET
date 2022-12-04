using System.Reflection;

namespace EntityDb.DocumentationGenerator.Models.Nodes;

public class AssemblyNode : Node
{
    public Assembly Assembly { get; }

    public AssemblyNode(Assembly assembly)
    {
        Assembly = assembly;
    }
}

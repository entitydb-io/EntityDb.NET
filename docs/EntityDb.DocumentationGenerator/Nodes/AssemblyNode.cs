using System.Reflection;

namespace EntityDb.DocumentationGenerator.Nodes;

public class AssemblyNode : Node
{
    public Assembly Assembly { get; }

    public AssemblyNode(Assembly assembly)
    {
        Assembly = assembly;
    }

    public string GetDescription()
    {
        var assemblyDescriptionAttribute = Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();

        if (assemblyDescriptionAttribute == null)
        {
            return "Missing Description";
        }

        return assemblyDescriptionAttribute.Description;
    }
}

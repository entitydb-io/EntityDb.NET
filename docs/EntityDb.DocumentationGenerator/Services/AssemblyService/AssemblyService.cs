using System.Reflection;

namespace EntityDb.DocumentationGenerator.Services.AssemblyService;

public class AssemblyService : IAssemblyService
{
    public IEnumerable<Assembly> GetAssemblies(DirectoryInfo directory)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();

        return directory.GetFiles($"EntityDb.*.dll")
            .Where(assemblyFile => !assemblyFile.Name.Contains(executingAssembly.GetName().Name!))
            .Select(assemblyFile => Assembly.LoadFrom(assemblyFile.FullName))
            .OrderBy(assembly => assembly.GetName().Name);
    }
}
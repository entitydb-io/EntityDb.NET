using System.Reflection;

namespace EntityDb.DocumentationGenerator.Services.TypeService;

public class TypeService : ITypeService
{
    public IEnumerable<Type> GetTypes(DirectoryInfo directory)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();

        return directory.GetFiles($"EntityDb.*.dll")
            .Where(assemblyFile => !assemblyFile.Name.Contains(executingAssembly.GetName().Name!))
            .Select(assemblyFile => Assembly.LoadFrom(assemblyFile.FullName))
            .SelectMany(assembly => assembly.GetTypes().Where(type => type.IsPublic))
            .OrderBy(type => type.Namespace);
    }
}
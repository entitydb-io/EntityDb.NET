using System.Reflection;

namespace EntityDb.DocumentationGenerator.Services.AssemblyService;

public class AssemblyService : IAssemblyService
{
    public Assembly? GetAssemblyOrDefault(DirectoryInfo directory, string fileName)
    {
        var assemblyFile = directory.GetFiles(fileName)
            .SingleOrDefault();

        if (assemblyFile == default)
        {
            return default;
        }

        return Assembly.LoadFrom(assemblyFile.FullName);
    }
}
using System.Reflection;

namespace EntityDb.DocumentationGenerator.Services.AssemblyService;

public class AssemblyService : IAssemblyService
{
    public Assembly GetAssembly(DirectoryInfo directory, string fileName)
    {
        var assemblyFile = directory.GetFiles(fileName)
            .SingleOrDefault();

        if (assemblyFile == default)
        {
            throw new Exception($"Cannot locate {fileName} in {directory}");
        }

        return Assembly.LoadFrom(assemblyFile.FullName);
    }
}
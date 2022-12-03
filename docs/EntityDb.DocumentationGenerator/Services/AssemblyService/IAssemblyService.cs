using System.Reflection;

namespace EntityDb.DocumentationGenerator.Services.AssemblyService;

internal interface IAssemblyService
{
    IEnumerable<Assembly> GetAssemblies(DirectoryInfo directory);
}

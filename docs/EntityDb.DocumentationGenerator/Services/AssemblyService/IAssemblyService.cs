using System.Reflection;

namespace EntityDb.DocumentationGenerator.Services.AssemblyService;

internal interface IAssemblyService
{
    Assembly? GetAssemblyOrDefault(DirectoryInfo directory, string fileName);
}

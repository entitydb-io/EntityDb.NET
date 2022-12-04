using System.Reflection;

namespace EntityDb.DocumentationGenerator.Services.AssemblyService;

internal interface IAssemblyService
{
    Assembly GetAssembly(DirectoryInfo directory, string fileName);
}

namespace EntityDb.DocumentationGenerator.Services.TypeService;

internal interface ITypeService
{
    IEnumerable<Type> GetTypes(DirectoryInfo directory);
}

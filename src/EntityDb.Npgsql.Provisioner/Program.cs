using EntityDb.Npgsql.Provisioner.Commands;
using System.CommandLine;

namespace EntityDb.Npgsql.Provisioner;

internal static class Program
{
    public static Task<int> Main(string[] args)
    {
#if DEBUG
        if (args.Length == 0)
        {
            Console.Write("Please enter args: ");

            var input = Console.ReadLine() ?? string.Empty;

            args = input.Split(' ');
        }
#endif

        var rootCommand = new RootCommand();

        CreateDatabase.AddTo(rootCommand);
        CreateTables.AddTo(rootCommand);
        CreateRole.AddTo(rootCommand);

        return rootCommand.InvokeAsync(args);
    }
}

using EntityDb.MongoDb.Provisioner.Commands;
using System;
using System.CommandLine;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Provisioner;

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

        CreateRole.AddTo(rootCommand);
        CreateUser.AddTo(rootCommand);
        CreateCollectionsCluster.AddTo(rootCommand);
        CreateCollectionsServerless.AddTo(rootCommand);
        CreateCollectionsDirect.AddTo(rootCommand);

        return rootCommand.InvokeAsync(args);
    }
}

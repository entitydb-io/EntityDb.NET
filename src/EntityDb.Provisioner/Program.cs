using EntityDb.Provisioner.Commands.MongoDb;
using System.CommandLine;

#if DEBUG
if (args.Length == 0)
{
    Console.Write("Please enter args: ");

    var input = Console.ReadLine() ?? string.Empty;

    args = input.Split(' ');
}
#endif

var rootCommand = new RootCommand();

MongoDbCommand.AddTo(rootCommand);

return await rootCommand.InvokeAsync(args);

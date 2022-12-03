using System.CommandLine;

namespace EntityDb.Provisioner.Commands.Npgsql;

internal class NpgsqlCommand
{
    public static void AddTo(Command parentCommand)
    {
        var npgsql = new Command("npgsql");

        CreateDatabase.AddTo(npgsql);
        CreateTables.AddTo(npgsql);
        CreateRole.AddTo(npgsql);

        parentCommand.AddCommand(npgsql);
    }
}

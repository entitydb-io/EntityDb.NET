using EntityDb.Npgsql.Provisioner.Extensions;
using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace EntityDb.Npgsql.Provisioner.Commands;

internal class CreateTables : CommandBase
{
    public record Arguments
    (
        string ConnectionString,
        string ServiceName,
        string ServicePassword
    );

    private static async Task Execute(Arguments arguments)
    {
        var npgsqlConnection = new NpgsqlConnection(arguments.ConnectionString);

        await npgsqlConnection.OpenAsync();

        await npgsqlConnection.ChangeDatabaseAsync(arguments.ServiceName.ToLowerInvariant());

        await npgsqlConnection.ProvisionTables();

        await npgsqlConnection.CloseAsync();
    }

    public static void AddTo(RootCommand rootCommand)
    {
        var createRole = new Command("create-tables")
        {
            Handler = CommandHandler.Create<Arguments>(Execute)
        };

        AddConnectionStringArgument(createRole);
        AddServiceNameArgument(createRole);

        rootCommand.AddCommand(createRole);
    }
}

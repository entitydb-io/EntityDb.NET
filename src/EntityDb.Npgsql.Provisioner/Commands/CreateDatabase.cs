using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace EntityDb.Npgsql.Provisioner.Commands;

internal class CreateDatabase : CommandBase
{
    public record Arguments
    (
        string ConnectionString,
        string ServiceName
    );

    private static async Task Execute(Arguments arguments)
    {
        var npgsqlConnection = new NpgsqlConnection(arguments.ConnectionString);

        await npgsqlConnection.OpenAsync();

        await new NpgsqlCommand($"CREATE DATABASE {arguments.ServiceName} IF NOT EXISTS", npgsqlConnection).ExecuteNonQueryAsync();

        await npgsqlConnection.CloseAsync();
    }

    public static void AddTo(RootCommand rootCommand)
    {
        var createCollectionsDirect = new Command("create-database")
        {
            Handler = CommandHandler.Create<Arguments>(Execute)
        };

        AddConnectionStringArgument(createCollectionsDirect);
        AddServiceNameArgument(createCollectionsDirect);

        rootCommand.AddCommand(createCollectionsDirect);
    }
}

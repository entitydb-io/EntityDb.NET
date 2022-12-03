using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace EntityDb.Provisioner.Commands.Npgsql;

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

        await new global::Npgsql.NpgsqlCommand($"CREATE DATABASE {arguments.ServiceName}", npgsqlConnection).ExecuteNonQueryAsync();

        await npgsqlConnection.CloseAsync();
    }

    public static void AddTo(Command parentCommand)
    {
        var createCollectionsDirect = new Command("create-database")
        {
            Handler = CommandHandler.Create<Arguments>(Execute)
        };

        AddConnectionStringArgument(createCollectionsDirect);
        AddServiceNameArgument(createCollectionsDirect);

        parentCommand.AddCommand(createCollectionsDirect);
    }
}

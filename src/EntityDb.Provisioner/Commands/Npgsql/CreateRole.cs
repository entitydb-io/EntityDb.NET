using Npgsql;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

namespace EntityDb.Provisioner.Commands.Npgsql;

internal class CreateRole : CommandBase
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

        var commands = new[]
        {
            $"DROP ROLE IF EXISTS {arguments.ServiceName}",
            $"CREATE USER {arguments.ServiceName} PASSWORD '{arguments.ServicePassword}'",
            $"GRANT SELECT, INSERT ON TABLE agentsignatures, commands, leases, tags TO {arguments.ServiceName}",
            $"GRANT DELETE ON TABLE leases, tags TO {arguments.ServiceName}"
        };

        foreach (var command in commands)
        {
            await new global::Npgsql.NpgsqlCommand(command, npgsqlConnection).ExecuteNonQueryAsync();
        }

        await npgsqlConnection.CloseAsync();
    }

    public static void AddTo(Command parentCommand)
    {
        var createRole = new Command("create-role")
        {
            Handler = CommandHandler.Create<Arguments>(Execute)
        };

        AddConnectionStringArgument(createRole);
        AddServiceNameArgument(createRole);
        AddServicePasswordArgument(createRole);

        parentCommand.AddCommand(createRole);
    }
}

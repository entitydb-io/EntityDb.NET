using System.CommandLine;
using System.Text.RegularExpressions;

namespace EntityDb.Provisioner.Commands.Npgsql;

internal abstract class CommandBase
{
    private static readonly Regex ServiceNameRegex = new("^[a-z][a-z]*$", RegexOptions.IgnoreCase);

    protected static void AddConnectionStringArgument(Command command)
    {
        var connectionString = new Argument<string>("connection-string")
        {
            Description = "The connection string for the database."
        };

        command.AddArgument(connectionString);
    }

    protected static void AddServiceNameArgument(Command command)
    {
        var serviceName = new Argument<string>("service-name")
        {
            Description = "The name of the service that will use this database."
        };

        serviceName.AddValidator(serviceNameResult =>
        {
            var serviceName = serviceNameResult.GetValueOrDefault<string>() ?? string.Empty;

            if (!ServiceNameRegex.IsMatch(serviceName))
            {
                serviceNameResult.ErrorMessage = "The service name must begin with an letter, and can only contain letters.";
            }
        });

        command.AddArgument(serviceName);
    }

    protected static void AddServicePasswordArgument(Command command)
    {
        var servicePassword = new Argument<string>("service-password")
        {
            Description = "The password for the service that will use this database."
        };

        command.AddArgument(servicePassword);
    }
}

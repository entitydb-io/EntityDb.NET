using EntityDb.SqlDb.Documents.AgentSignature;
using EntityDb.SqlDb.Documents.Command;
using EntityDb.SqlDb.Documents.Lease;
using EntityDb.SqlDb.Documents.Tag;
using Npgsql;

namespace EntityDb.Npgsql.Provisioner.Extensions;

/// <summary>
///     Extensions for the Npgsql Connection.
/// </summary>
public static class NpgsqlConnectionExtensions
{
    private static readonly string[] Commands =
    {
        "CREATE COLLATION IF NOT EXISTS numeric (provider = icu, locale = 'en-u-kn-true');",

        "CREATE extension IF NOT EXISTS \"uuid-ossp\"",

        $"CREATE TABLE IF NOT EXISTS AgentSignatures (" +
            $"{nameof(AgentSignatureDocument.Id)} uuid DEFAULT uuid_generate_v4() PRIMARY KEY, " +
            $"{nameof(AgentSignatureDocument.TransactionId)} uuid NOT NULL, " +
            $"{nameof(AgentSignatureDocument.TransactionTimeStamp)} timestamp with time zone NOT NULL, " +
            $"{nameof(AgentSignatureDocument.EntityIds)} uuid[] NOT NULL, " +
            $"{nameof(AgentSignatureDocument.DataType)} varchar NOT NULL, " +
            $"{nameof(AgentSignatureDocument.Data)} jsonb NOT NULL " +
        $")",

        $"CREATE UNIQUE INDEX IF NOT EXISTS UniqueAgentSignatures ON AgentSignatures ({nameof(AgentSignatureDocument.TransactionId)})",

        $"CREATE TABLE IF NOT EXISTS Commands (" +
            $"{nameof(CommandDocument.Id)} uuid DEFAULT uuid_generate_v4() PRIMARY KEY, " +
            $"{nameof(CommandDocument.TransactionId)} uuid NOT NULL, " +
            $"{nameof(CommandDocument.TransactionTimeStamp)} timestamp with time zone NOT NULL, " +
            $"{nameof(CommandDocument.EntityId)} uuid NOT NULL, " +
            $"{nameof(CommandDocument.EntityVersionNumber)} bigint NOT NULL, " +
            $"{nameof(CommandDocument.DataType)} varchar NOT NULL, " +
            $"{nameof(CommandDocument.Data)} jsonb NOT NULL " +
        $")",

        $"CREATE UNIQUE INDEX IF NOT EXISTS UniqueCommands ON Commands ({nameof(CommandDocument.EntityId)}, {nameof(CommandDocument.EntityVersionNumber)})",

        $"CREATE TABLE IF NOT EXISTS Leases (" +
            $"{nameof(LeaseDocument.Id)} uuid DEFAULT uuid_generate_v4() PRIMARY KEY, " +
            $"{nameof(LeaseDocument.TransactionId)} uuid NOT NULL, " +
            $"{nameof(LeaseDocument.TransactionTimeStamp)} timestamp with time zone NOT NULL, " +
            $"{nameof(LeaseDocument.EntityId)} uuid NOT NULL, " +
            $"{nameof(LeaseDocument.EntityVersionNumber)} bigint NOT NULL, " +
            $"{nameof(LeaseDocument.DataType)} varchar NOT NULL, " +
            $"{nameof(LeaseDocument.Data)} jsonb NOT NULL, " +
            $"{nameof(LeaseDocument.Scope)} varchar NOT NULL, " +
            $"{nameof(LeaseDocument.Label)} varchar NOT NULL, " +
            $"{nameof(LeaseDocument.Value)} varchar NOT NULL " +
        $")",

        $"CREATE UNIQUE INDEX IF NOT EXISTS UniqueLeases ON Leases ({nameof(LeaseDocument.Scope)}, {nameof(LeaseDocument.Label)}, {nameof(LeaseDocument.Value)})",

        $"CREATE TABLE IF NOT EXISTS Tags (" +
            $"{nameof(TagDocument.Id)} uuid DEFAULT uuid_generate_v4() PRIMARY KEY, " +
            $"{nameof(TagDocument.TransactionId)} uuid NOT NULL, " +
            $"{nameof(TagDocument.TransactionTimeStamp)} timestamp with time zone NOT NULL, " +
            $"{nameof(TagDocument.EntityId)} uuid NOT NULL, " +
            $"{nameof(TagDocument.EntityVersionNumber)} bigint NOT NULL, " +
            $"{nameof(TagDocument.DataType)} varchar NOT NULL, " +
            $"{nameof(TagDocument.Data)} jsonb NOT NULL, " +
            $"{nameof(TagDocument.Label)} varchar NOT NULL, " +
            $"{nameof(TagDocument.Value)} varchar NOT NULL " +
        $")",

        $"CREATE INDEX IF NOT EXISTS TagLookup ON Tags ({nameof(TagDocument.Label)}, {nameof(TagDocument.Value)})",
    };

    /// <summary>
    ///     Provisions the needed collections on the database.
    /// </summary>
    /// <param name="npgsqlConnection">The npgsql connection.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An asynchronous task that, when complete, signals that the tables have been provisioned.</returns>
    /// <remarks>
    ///     You should ONLY use this in your code for integration testing. Real databases should be provisioned using the
    ///     dotnet tool EntityDb.Npgsql.Provisioner
    /// </remarks>
    public static async Task ProvisionTables(this NpgsqlConnection npgsqlConnection,
        CancellationToken cancellationToken = default)
    {
        await npgsqlConnection.OpenAsync(cancellationToken);

        foreach (var command in Commands)
        {
            var dbCommand = new NpgsqlCommand(command, npgsqlConnection);

            await dbCommand.ExecuteNonQueryAsync(cancellationToken);
        }

        await npgsqlConnection.CloseAsync();
    }
}

using EntityDb.Npgsql.Provisioner.Extensions;
using Npgsql;

// Global Args

var host = "localhost";
var port = 5432;


// Args

var serviceName = "entitydb";
var servicePassword = "entitydb";

//// Handler

//var rootConnectionString = $"Host={host};Port={port};Username={rootUsername};Password={rootPassword};Include Error Detail=true";

//var rootDbConnection = new NpgsqlConnection(rootConnectionString);

//await rootDbConnection.OpenAsync();

//var rootCommands = new string[]
//{
//    $"CREATE DATABASE {serviceName}",
//    $"CREATE USER {serviceName} PASSWORD '{servicePassword}'",
//};

//foreach (var rootCommand in rootCommands)
//{
//    await new NpgsqlCommand(rootCommand, rootDbConnection).ExecuteNonQueryAsync();
//}


//await rootDbConnection.CloseAsync();





// Args

// Handler

var scopedConnectionString = $"Host={host};Port={port};Username={serviceName};Password={servicePassword};Include Error Detail=true;Database={serviceName}";

var scopedDbConnection = new NpgsqlConnection(scopedConnectionString);

await scopedDbConnection.ProvisionTables();

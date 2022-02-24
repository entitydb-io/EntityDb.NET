using System.Text.Json.Serialization;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;

internal class MongoDbAtlasUserRole
{
    [JsonPropertyName("databaseName")] public string? DatabaseName { get; set; }

    [JsonPropertyName("roleName")] public string? RoleName { get; set; }
}

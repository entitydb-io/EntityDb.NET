using System.Text.Json.Serialization;

namespace EntityDb.Provisioner.MongoDbAtlas.Models;

internal sealed class MongoDbAtlasUserRole
{
    [JsonPropertyName("databaseName")] public string? DatabaseName { get; set; }

    [JsonPropertyName("roleName")] public string? RoleName { get; set; }
}

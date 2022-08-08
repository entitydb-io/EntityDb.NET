using System.Text.Json.Serialization;

namespace EntityDb.Provisioner.MongoDbAtlas.Models;

internal class MongoDbAtlasResource
{
    [JsonPropertyName("db")] public string? Db { get; set; }

    [JsonPropertyName("collection")] public string? Collection { get; set; }

    [JsonPropertyName("cluster")] public bool? Cluster { get; set; }
}

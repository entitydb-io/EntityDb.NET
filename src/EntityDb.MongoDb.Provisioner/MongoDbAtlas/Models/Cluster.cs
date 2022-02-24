using System.Text.Json.Serialization;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;

internal class Cluster
{
    [JsonPropertyName("srvAddress")] public string? SrvAddress { get; set; }
}

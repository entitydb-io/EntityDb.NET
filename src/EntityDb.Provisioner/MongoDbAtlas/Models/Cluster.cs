using System.Text.Json.Serialization;

namespace EntityDb.Provisioner.MongoDbAtlas.Models;

internal class Cluster
{
    [JsonPropertyName("srvAddress")] public string? SrvAddress { get; set; }
}

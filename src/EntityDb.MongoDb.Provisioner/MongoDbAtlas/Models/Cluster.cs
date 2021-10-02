using System.Text.Json.Serialization;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models
{
    public class Cluster
    {
        [JsonPropertyName("srvAddress")]
        public string? SrvAddress { get; set; }
    }
}

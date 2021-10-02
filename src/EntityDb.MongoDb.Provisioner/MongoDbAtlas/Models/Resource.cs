using System.Text.Json.Serialization;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models
{
    public class MongoDbAtlasResource
    {
        [JsonPropertyName("db")]
        public string? Db { get; set; }

        [JsonPropertyName("collection")]
        public string? Collection { get; set; }

        [JsonPropertyName("cluster")]
        public bool? Cluster { get; set; }
    }
}

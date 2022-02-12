using System.Text.Json.Serialization;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models
{
    internal class Group
    {
        [JsonPropertyName("id")] public string Id { get; set; } = "";

        [JsonPropertyName("name")] public string Name { get; set; } = "";
    }
}

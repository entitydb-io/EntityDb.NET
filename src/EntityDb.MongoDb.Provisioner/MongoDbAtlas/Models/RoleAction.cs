using System.Text.Json.Serialization;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models
{
    public class MongoDbAtlasRoleAction
    {
        [JsonPropertyName("action")] public string? Action { get; set; }

        [JsonPropertyName("resources")] public MongoDbAtlasResource[]? Resources { get; set; }
    }
}

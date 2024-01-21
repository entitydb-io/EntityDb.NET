using System.Text.Json.Serialization;

namespace EntityDb.Provisioner.MongoDbAtlas.Models;

internal sealed class MongoDbAtlasRoleAction
{
    [JsonPropertyName("action")] public string? Action { get; set; }

    [JsonPropertyName("resources")] public MongoDbAtlasResource[]? Resources { get; set; }
}

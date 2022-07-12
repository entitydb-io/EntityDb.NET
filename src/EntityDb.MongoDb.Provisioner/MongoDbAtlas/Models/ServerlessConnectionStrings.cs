using System.Text.Json.Serialization;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;

internal class ServerlessConnectionStrings
{
    [JsonPropertyName("standardSrv")] public string? StandardSrv { get; set; }
}

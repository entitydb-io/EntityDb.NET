using System.Text.Json.Serialization;

namespace EntityDb.Provisioner.MongoDbAtlas.Models;

internal sealed class ServerlessConnectionStrings
{
    [JsonPropertyName("standardSrv")] public string? StandardSrv { get; set; }
}

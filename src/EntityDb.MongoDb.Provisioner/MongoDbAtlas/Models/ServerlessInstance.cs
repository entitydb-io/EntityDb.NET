using System.Text.Json.Serialization;

namespace EntityDb.MongoDb.Provisioner.MongoDbAtlas.Models;

internal class ServerlessInstance
{
    [JsonPropertyName("connectionStrings")] public ServerlessConnectionStrings? ConnectionStrings { get; set; }
}

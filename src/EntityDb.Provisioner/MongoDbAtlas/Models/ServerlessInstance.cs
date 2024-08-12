using System.Text.Json.Serialization;

namespace EntityDb.Provisioner.MongoDbAtlas.Models;

internal sealed class ServerlessInstance
{
    [JsonPropertyName("connectionStrings")]
    public ServerlessConnectionStrings? ConnectionStrings { get; set; }
}

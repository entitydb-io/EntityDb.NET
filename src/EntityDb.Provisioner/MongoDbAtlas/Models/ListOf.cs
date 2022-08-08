using System.Text.Json.Serialization;

namespace EntityDb.Provisioner.MongoDbAtlas.Models;

internal class ListOf<T>
{
    [JsonPropertyName("results")] public T[] Results { get; set; } = Array.Empty<T>();

    [JsonPropertyName("totalCount")] public ulong TotalCount { get; set; }
}

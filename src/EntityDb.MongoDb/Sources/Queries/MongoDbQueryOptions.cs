using MongoDB.Driver;

namespace EntityDb.MongoDb.Sources.Queries;

/// <summary>
///     Options for configuring queries in MongoDb.
/// </summary>
public sealed class MongoDbQueryOptions
{
    /// <summary>
    ///     Options for finding documents.
    /// </summary>
    public FindOptions? FindOptions { get; set; }
}

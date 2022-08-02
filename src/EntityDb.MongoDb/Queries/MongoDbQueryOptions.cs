using MongoDB.Driver;

namespace EntityDb.MongoDb.Queries;

/// <summary>
///     Options for configuring queries in MongoDb.
/// </summary>
public class MongoDbQueryOptions
{
    /// <summary>
    ///     Options for finding documents.
    /// </summary>
    public FindOptions? FindOptions { get; set; }
}

using System.Data.Common;

namespace EntityDb.SqlDb.Documents;

internal interface IDocumentReader
{
    string[] GetPropertyNames();
}

internal interface IDocumentReader<TDocument> : IDocumentReader
{
    Task<TDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken);
}

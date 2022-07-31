using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents;

internal interface IDocumentReader
{
    string[] GetPropertyNames();
}

internal interface IDocumentReader<TDocument> : IDocumentReader
{
    Task<TDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken);
}

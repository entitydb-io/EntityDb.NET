using System.Data.Common;

namespace EntityDb.SqlDb.Documents.AgentSignature;

internal class AgentSignatureDataDocumentReader : AgentSignatureDocumentReaderBase, IDocumentReader<AgentSignatureDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(AgentSignatureDocument.Data),
    };

    public AgentSignatureDataDocumentReader() : base(_propertyNames)
    {
    }

    public async Task<AgentSignatureDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new AgentSignatureDocument
        {
            Data = await dbDataReader.GetFieldValueAsync<string>(_dataOrdinal)
        };
    }
}

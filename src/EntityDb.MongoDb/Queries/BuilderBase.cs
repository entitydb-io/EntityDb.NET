using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Documents;

namespace EntityDb.MongoDb.Queries;

internal abstract class BuilderBase
{
    protected const string DataTypeNameFieldName =
        $"{nameof(DocumentBase.Data)}.{nameof(DocumentBase.Data.Headers)}.{EnvelopeHelper.Type}";

    protected const string DataValueFieldName =
        $"{nameof(DocumentBase.Data)}.{nameof(DocumentBase.Data.Value)}";
}

using EntityDb.Common.Envelopes;
using EntityDb.MongoDb.Documents;

namespace EntityDb.MongoDb.Queries
{
    internal abstract class BuilderBase
    {
        protected static readonly string DataTypeNameFieldName =
            $"{nameof(DocumentBase.Data)}.{nameof(DocumentBase.Data.Headers)}.{EnvelopeHelper.Type}";

        protected static readonly string DataValueFieldName =
            $"{nameof(DocumentBase.Data)}.{nameof(DocumentBase.Data.Value)}";
    }
}

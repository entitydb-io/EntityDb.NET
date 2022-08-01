using MongoDB.Bson;
using MongoDB.Bson.IO;

namespace EntityDb.MongoDb.Rewriters;

internal class HoistedRewriter : BsonDocumentRewriter
{
    private readonly string[] _hoistedFieldNames;
    private readonly string _parentFieldName;

    private bool _foundTopDocument;

    public HoistedRewriter(BsonWriter bsonWriter, string parentFieldName, string[] hoistedFieldNames) :
        base(bsonWriter)
    {
        _parentFieldName = parentFieldName;
        _hoistedFieldNames = hoistedFieldNames;
    }

    protected override void RewriteDocument(BsonElement[] bsonElements)
    {
        if (_foundTopDocument)
        {
            base.RewriteDocument(bsonElements);
        }
        else
        {
            _foundTopDocument = true;

            RewriteHoisted(bsonElements);
        }
    }

    private void RewriteHoisted(IEnumerable<BsonElement> bsonElements)
    {
        BsonWriter.WriteStartDocument();

        foreach (var bsonElement in bsonElements)
        {
            if (_hoistedFieldNames.Contains(bsonElement.Name))
            {
                BsonWriter.WriteName(bsonElement.Name);
            }
            else
            {
                BsonWriter.WriteName(_parentFieldName + "." + bsonElement.Name);
            }

            Rewrite(bsonElement.Value);
        }

        BsonWriter.WriteEndDocument();
    }
}

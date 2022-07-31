﻿using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Tag;

internal class TagTransactionIdDocumentReader : IDocumentReader<TagDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(TagDocument.TransactionId),
    };

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<TagDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new TagDocument
        {
            TransactionId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(0))
        };
    }
}
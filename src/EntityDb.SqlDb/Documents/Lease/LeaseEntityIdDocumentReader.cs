﻿using EntityDb.Abstractions.ValueObjects;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace EntityDb.SqlDb.Documents.Lease;

internal class LeaseEntityIdDocumentReader : IDocumentReader<LeaseDocument>
{
    private static readonly string[] _propertyNames =
    {
        nameof(LeaseDocument.EntityId),
    };

    public string[] GetPropertyNames() => _propertyNames;

    public async Task<LeaseDocument> Read(DbDataReader dbDataReader, CancellationToken cancellationToken)
    {
        return new LeaseDocument
        {
            EntityId = new Id(await dbDataReader.GetFieldValueAsync<Guid>(0))
        };
    }
}
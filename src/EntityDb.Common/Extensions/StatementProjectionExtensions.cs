using EntityDb.Abstractions.Commands;
using EntityDb.Abstractions.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntityDb.Common.Extensions;

public static class ProjectionExtensions
{
    public static TProjection Reduce<TProjection>(this TProjection projection, Guid entityId, IStatement<TProjection> statement)
    {
        return statement.Reduce(entityId, projection);
    }

    public static TProjection Reduce<TProjection>(this TProjection projection, Guid entityId, IEnumerable<IStatement<TProjection>> statements)
    {
        return statements.Aggregate(projection, (previousProjection, nextStatement) => previousProjection.Reduce(entityId, nextStatement));
    }
}

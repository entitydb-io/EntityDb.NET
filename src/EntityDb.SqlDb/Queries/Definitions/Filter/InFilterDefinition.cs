using System.Collections;

namespace EntityDb.SqlDb.Queries.Definitions.Filter;

internal record struct InFilterDefinition(string PropertyName, IEnumerable PropertyValues) : IFilterDefinition;

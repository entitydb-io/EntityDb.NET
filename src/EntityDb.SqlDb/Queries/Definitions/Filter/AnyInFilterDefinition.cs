using System.Collections;

namespace EntityDb.SqlDb.Queries.Definitions.Filter;

internal record struct AnyInFilterDefinition(string PropertyName, IEnumerable PropertyValues) : IFilterDefinition;

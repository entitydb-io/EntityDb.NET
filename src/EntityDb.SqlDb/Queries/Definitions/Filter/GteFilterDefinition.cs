namespace EntityDb.SqlDb.Queries.Definitions.Filter;

internal record struct GteFilterDefinition(string PropertyName, object PropertyValue) : IFilterDefinition;

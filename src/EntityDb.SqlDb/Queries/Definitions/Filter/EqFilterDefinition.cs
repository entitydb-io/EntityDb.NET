namespace EntityDb.SqlDb.Queries.Definitions.Filter;

internal record struct EqFilterDefinition(string PropertyName, object PropertyValue) : IFilterDefinition;

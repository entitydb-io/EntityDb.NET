namespace EntityDb.SqlDb.Queries.Definitions.Filter;

internal record struct LteFilterDefinition(string PropertyName, object PropertyValue) : IFilterDefinition;

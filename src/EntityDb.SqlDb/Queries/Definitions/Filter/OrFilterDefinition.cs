namespace EntityDb.SqlDb.Queries.Definitions.Filter;

internal record struct OrFilterDefinition(IFilterDefinition[] FilterDefinitions) : IFilterDefinition;

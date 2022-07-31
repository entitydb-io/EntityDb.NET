namespace EntityDb.SqlDb.Queries.Definitions.Filter;

internal record struct AndFilterDefinition(IFilterDefinition[] FilterDefinitions) : IFilterDefinition;

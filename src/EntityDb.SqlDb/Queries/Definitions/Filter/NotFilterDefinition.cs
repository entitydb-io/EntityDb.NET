namespace EntityDb.SqlDb.Queries.Definitions.Filter;

internal record struct NotFilterDefinition(IFilterDefinition FilterDefinition) : IFilterDefinition;

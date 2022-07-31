namespace EntityDb.SqlDb.Queries.Definitions.Sort;

internal record struct CombineSortDefinition(ISortDefinition[] SortDefinitions) : ISortDefinition;

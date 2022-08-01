namespace EntityDb.SqlDb.Queries.Definitions.Sort;

internal record struct AscSortDefinition(string PropertyName) : ISortDefinition;

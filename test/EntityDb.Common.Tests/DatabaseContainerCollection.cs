using Xunit;

namespace EntityDb.Common.Tests;

[CollectionDefinition(nameof(DatabaseContainerCollection))]
public class DatabaseContainerCollection : ICollectionFixture<DatabaseContainerFixture>
{
}

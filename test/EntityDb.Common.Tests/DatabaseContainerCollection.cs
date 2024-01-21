using Xunit;

namespace EntityDb.Common.Tests;

[CollectionDefinition(nameof(DatabaseContainerCollection))]
public sealed class DatabaseContainerCollection : ICollectionFixture<DatabaseContainerFixture>;

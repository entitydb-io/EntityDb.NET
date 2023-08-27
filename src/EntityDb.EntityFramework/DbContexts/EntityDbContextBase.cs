using EntityDb.Abstractions.ValueObjects;
using EntityDb.EntityFramework.Converters;
using Microsoft.EntityFrameworkCore;

namespace EntityDb.EntityFramework.DbContexts;

/// <summary>
///     A DbContext that adds basic converters for types defined in <see cref="Abstractions.ValueObjects"/>
/// </summary>
public abstract class EntityDbContextBase : DbContext
{
    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Id>()
            .HaveConversion<IdConverter>();

        configurationBuilder
            .Properties<VersionNumber>()
            .HaveConversion<VersionNumberConverter>();

        configurationBuilder
            .Properties<TimeStamp>()
            .HaveConversion<TimeStampConverter>();
    }
}

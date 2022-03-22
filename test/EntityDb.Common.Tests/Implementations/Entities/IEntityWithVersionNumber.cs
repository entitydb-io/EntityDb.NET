using EntityDb.Abstractions.ValueObjects;

namespace EntityDb.Common.Tests.Implementations.Entities;

public interface IEntityWithVersionNumber<TEntity>
{
    static abstract TEntity Construct(Id id, VersionNumber versionNumber);
}
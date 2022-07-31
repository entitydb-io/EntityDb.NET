using System;
using System.IO;
using System.Threading.Tasks;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Xunit;

namespace EntityDb.Common.Tests;

public class DatabaseContainerFixture : IAsyncLifetime
{
    private static readonly RedisTestcontainerConfiguration _redisConfiguration = new("redis:7.0.2");

    private static readonly MongoDbTestcontainerConfiguration _mongoDbConfiguration = new("mongo:5.0.9")
    {
        Database = "entitydb",
        Username = null,
        Password = null
    };

    private static readonly string DockerVolumeMongoDbInit = Path.Combine
    (
        AppDomain.CurrentDomain.BaseDirectory,
        "DockerVolumes",
        "MongoDb",
        "Init"
    );

    private static readonly PostgreSqlTestcontainerConfiguration _postgreSqlConfiguration = new("postgres:12.6")
    {
        Database = "entitydb",
        Username = "entitydb",
        Password = "entitydb",
    };

    public RedisTestcontainerConfiguration RedisConfiguration => _redisConfiguration;

    public MongoDbTestcontainerConfiguration MongoDbConfiguration => _mongoDbConfiguration;

    public PostgreSqlTestcontainerConfiguration PostgreSqlConfiguration => _postgreSqlConfiguration;

    public RedisTestcontainer RedisContainer { get; } = new TestcontainersBuilder<RedisTestcontainer>()
        .WithDatabase(_redisConfiguration)
        .Build();

    public MongoDbTestcontainer MongoDbContainer { get; } = new TestcontainersBuilder<MongoDbTestcontainer>()
        .WithDatabase(_mongoDbConfiguration)
        .WithBindMount(DockerVolumeMongoDbInit, "/docker-entrypoint-initdb.d")
        .WithCommand("--replSet", "entitydb")
        .Build();

    public PostgreSqlTestcontainer PostgreSqlContainer { get; } = new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(_postgreSqlConfiguration)
            .Build();

    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();
        await MongoDbContainer.StartAsync();
        await PostgreSqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await RedisContainer.DisposeAsync();
        await MongoDbContainer.DisposeAsync();
        await PostgreSqlContainer.DisposeAsync();
    }
}

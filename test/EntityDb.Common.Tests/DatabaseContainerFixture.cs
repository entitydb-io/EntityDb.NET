using Testcontainers.MongoDb;
using Testcontainers.Redis;
using Xunit;

namespace EntityDb.Common.Tests;

public class DatabaseContainerFixture : IAsyncLifetime
{
    public const string OmniParameter = "entitydb";

    private static readonly string DockerVolumeMongoDbInit = Path.Combine
    (
        AppDomain.CurrentDomain.BaseDirectory,
        "DockerVolumes",
        "MongoDb",
        "Init"
    );

    public RedisContainer RedisContainer { get; } = new RedisBuilder()
        .WithImage("redis:7.2.0")
        .Build();

    public MongoDbContainer MongoDbContainer { get; } = new MongoDbBuilder()
        .WithImage("mongo:7.0.0")
        .WithUsername(null)
        .WithPassword(null)
        .WithBindMount(DockerVolumeMongoDbInit, "/docker-entrypoint-initdb.d")
        .WithCommand("--replSet", OmniParameter)
        .Build();

    public async Task InitializeAsync()
    {
        await RedisContainer.StartAsync();
        await MongoDbContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await RedisContainer.DisposeAsync();
        await MongoDbContainer.DisposeAsync();
    }
}

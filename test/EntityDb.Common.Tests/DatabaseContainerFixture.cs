using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
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

    public IContainer MongoDbContainer { get; } = new ContainerBuilder()
        .WithImage("mongo:7.0.0")
        .WithPortBinding(27017, true)
        .WithBindMount(DockerVolumeMongoDbInit, "/docker-entrypoint-initdb.d")
        .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", null)
        .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", null)
        .WithCommand("--replSet", OmniParameter)
        .WithWaitStrategy(Wait.ForUnixContainer().AddCustomWaitStrategy(new WaitUntil()))
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

    private sealed class WaitUntil : IWaitUntil
    {
        private static readonly string[] LineEndings = { "\r\n", "\n" };

        public async Task<bool> UntilAsync(IContainer container)
        {
            var (text, text2) = await container.GetLogs(timestampsEnabled: false).ConfigureAwait(false);
            return 2.Equals(Array.Empty<string>().Concat(text.Split(LineEndings, StringSplitOptions.RemoveEmptyEntries))
                .Concat(text2.Split(LineEndings, StringSplitOptions.RemoveEmptyEntries))
                .Count(line => line.Contains("Waiting for connections")));
        }
    }
}
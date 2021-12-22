using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions
{
    internal record WriteRedisSession(IConnectionMultiplexer ConnectionMultiplexer) : IRedisSession
    {
        private static CommandFlags GetCommandFlags()
        {
            return CommandFlags.DemandMaster;
        }

        public async Task<bool> Insert(RedisKey redisKey, RedisValue redisValue)
        {
            var redisTransaction = ConnectionMultiplexer.GetDatabase().CreateTransaction();

            var insertedTask = redisTransaction.StringSetAsync(redisKey, redisValue);

            await redisTransaction.ExecuteAsync(GetCommandFlags());

            return await insertedTask;
        }

        public async Task<RedisValue> Find(RedisKey redisKey)
        {
            var redisDatabase = ConnectionMultiplexer.GetDatabase();

            return await redisDatabase.StringGetAsync(redisKey, GetCommandFlags());
        }

        public async Task<bool> Delete(IEnumerable<RedisKey> redisKeys)
        {
            var redisTransaction = ConnectionMultiplexer.GetDatabase().CreateTransaction();

            var deleteSnapshotTasks = redisKeys
                .Select(key => redisTransaction.KeyDeleteAsync(key))
                .ToArray();

            await redisTransaction.ExecuteAsync(GetCommandFlags());

            await Task.WhenAll(deleteSnapshotTasks);

            return deleteSnapshotTasks.All(task => task.Result);
        }

        public virtual ValueTask DisposeAsync()
        {
            ConnectionMultiplexer.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}

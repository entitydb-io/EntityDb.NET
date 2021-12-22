using EntityDb.Common.Exceptions;
using EntityDb.Common.Snapshots;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EntityDb.Redis.Sessions
{
    internal record ReadOnlyRedisSession(IConnectionMultiplexer ConnectionMultiplexer, SnapshotSessionOptions SnapshotSessionOptions) : IRedisSession
    {
        //TODO: Cover this with base snapshot repository tests
        private CommandFlags GetCommandFlags()
        {
            if (SnapshotSessionOptions.SecondaryPreferred)
            {
                return CommandFlags.PreferReplica;
            }

            return CommandFlags.PreferMaster;
        }

        public Task<bool> Insert(RedisKey redisKey, RedisValue redisValue)
        {
            throw new CannotWriteInReadOnlyModeException();
        }

        public async Task<RedisValue> Find(RedisKey redisKey)
        {
            var redisDatabase = ConnectionMultiplexer.GetDatabase();

            return await redisDatabase.StringGetAsync(redisKey, GetCommandFlags());
        }

        public Task<bool> Delete(IEnumerable<RedisKey> redisKeys)
        {
            throw new CannotWriteInReadOnlyModeException();
        }

        public virtual ValueTask DisposeAsync()
        {
            ConnectionMultiplexer.Dispose();

            return ValueTask.CompletedTask;
        }
    }
}

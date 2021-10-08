using EntityDb.Abstractions.Commands;
using EntityDb.TestImplementations.Commands;
using EntityDb.TestImplementations.Entities;

namespace EntityDb.TestImplementations.Seeders
{
    public static class CommandSeeder
    {
        public static ICommand<TransactionEntity> Create()
        {
            return new DoNothing();
        }
    }
}

using EntityDb.Abstractions.Commands;
using EntityDb.Common.Tests.Implementations.Commands;
using EntityDb.Common.Tests.Implementations.Entities;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public static class CommandSeeder
{
    public static ICommand<TransactionEntity> Create()
    {
        return new DoNothing();
    }
}
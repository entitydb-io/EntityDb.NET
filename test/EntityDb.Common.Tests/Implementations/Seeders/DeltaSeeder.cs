using EntityDb.Abstractions.Sources.Attributes;
using EntityDb.Common.Tests.Implementations.Deltas;

namespace EntityDb.Common.Tests.Implementations.Seeders;

public interface IDeltaSeeder
{
    object Create(ulong versionNumber);
}

public class AddTagSeeder : IDeltaSeeder
{
    private readonly ITag _tag;

    public AddTagSeeder(ITag tag)
    {
        _tag = tag;
    }
    
    public object Create(ulong versionNumber)
    {
        return new AddTag(_tag);
    }
}

public class DeleteTagSeeder : IDeltaSeeder
{
    private readonly ITag _tag;

    public DeleteTagSeeder(ITag tag)
    {
        _tag = tag;
    }
    
    public object Create(ulong versionNumber)
    {
        return new DeleteTag(_tag);
    }
}

public class AddLeaseSeeder : IDeltaSeeder
{
    private readonly ILease _lease;

    public AddLeaseSeeder(ILease lease)
    {
        _lease = lease;
    }

    public object Create(ulong versionNumber)
    {
        return new AddLease(_lease);
    }
}

public class DeleteLeaseSeeder : IDeltaSeeder
{
    private readonly ILease _lease;

    public DeleteLeaseSeeder(ILease lease)
    {
        _lease = lease;
    }

    public object Create(ulong versionNumber)
    {
        return new DeleteLease(_lease);
    }
}

public class StoreNumberSeeder : IDeltaSeeder
{
    public object Create(ulong versionNumber)
    {
        return new StoreNumber(versionNumber);
    }
}

public static class DeltaSeeder
{
    public static object Create()
    {
        return new DoNothing();
    }
}
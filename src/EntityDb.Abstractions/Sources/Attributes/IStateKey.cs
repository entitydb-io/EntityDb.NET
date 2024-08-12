namespace EntityDb.Abstractions.Sources.Attributes;

public interface IStateKey
{
    ILease ToLease();
}

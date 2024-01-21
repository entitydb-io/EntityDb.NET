namespace EntityDb.Abstractions.Sources.Attributes;

public interface IMessageKey
{
    ILease ToLease(IStateKey stateKey);
}

using System.Threading.Tasks;

namespace EntityDb.MongoDb.Commands;

abstract record DocumentsCommand
{
    public abstract Task Execute();
}

using System.Collections.Generic;

namespace EntityDb.MongoDb.Transactions
{
    internal class TestModeTransactionManager
    {
        private readonly List<string> _holds = new();

        public void Hold(string id)
        {
            _holds.Add(id);
        }

        public void Release(string id)
        {
            _holds.Remove(id);
        }

        public bool NoHolds()
        {
            return _holds.Count == 0;
        }
    }
}

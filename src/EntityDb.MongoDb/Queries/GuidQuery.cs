using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EntityDb.MongoDb.Queries
{
    internal abstract record GuidQuery<TDocument> : DocumentQuery<TDocument>
    {
        public int? DistinctSkip { get; init; }
        public int? DistinctLimit { get; init; }

        protected abstract IEnumerable<Guid> MapToGuids(IEnumerable<TDocument> documents);

        public async Task<Guid[]> GetDistinctGuids()
        {
            var documents = await GetDocuments();

            var guids = MapToGuids(documents).Distinct();

            if (DistinctSkip.HasValue)
            {
                guids = guids.Skip(DistinctSkip.Value);
            }

            if (DistinctLimit.HasValue)
            {
                guids = guids.Take(DistinctLimit.Value);
            }

            return guids.ToArray();
        }
    }
}

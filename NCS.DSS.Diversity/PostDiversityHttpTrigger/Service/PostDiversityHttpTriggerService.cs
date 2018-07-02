using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public class PostDiversityHttpTriggerService : IPostDiversityHttpTriggerService
    {
        public async Task<Guid?> CreateAsync(Models.Diversity diversity)
        {
            if (diversity == null)
                return null;

            var databaseClient = new DatabaseClient();

            var collectionLink = databaseClient.CreateCollectionLink();

            var diversityId = Guid.NewGuid();
            diversity.DiversityId = diversityId;

            var client =  databaseClient.CreateDocumentClient();

            var created = await client.CreateDocumentAsync(collectionLink, diversity);

            return created.StatusCode == HttpStatusCode.Created ? diversityId : (Guid?)null;
        }
    }
}

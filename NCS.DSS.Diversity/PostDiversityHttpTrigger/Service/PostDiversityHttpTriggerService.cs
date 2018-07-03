using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public class PostDiversityHttpTriggerService : IPostDiversityHttpTriggerService
    {
        public async Task<Guid?> CreateAsync(Models.Diversity diversity)
        {
            if (diversity == null)
                return null;

            var diversityId = Guid.NewGuid();
            diversity.DiversityId = diversityId;

            var documentDbProvider = new DocumentDBProvider();
            var created = await documentDbProvider.CreatDiversityDetailAsync(diversity);

            return created.StatusCode == HttpStatusCode.Created ? diversityId : (Guid?)null;

        }
    }
}

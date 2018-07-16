using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public class PostDiversityHttpTriggerService : IPostDiversityHttpTriggerService
    {
        public async Task<Models.Diversity> CreateAsync(Models.Diversity diversity)
        {
            if (diversity == null)
                return null;

            var diversityId = Guid.NewGuid();
            diversity.DiversityId = diversityId;

            if (!diversity.LastModifiedDate.HasValue)
                diversity.LastModifiedDate = DateTime.Now;

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.CreateDiversityDetailAsync(diversity);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic) response.Resource : null;

        }
    }
}

using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public class PostDiversityHttpTriggerService : IPostDiversityHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;

        public PostDiversityHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public bool DoesDiversityDetailsExistForCustomer(Guid customerId)
        {
            return _documentDbProvider.DoesDiversityDetailsExistForCustomer(customerId);
        }

        public async Task<Models.Diversity> CreateAsync(Models.Diversity diversity)
        {
            if (diversity == null)
                return null;

            diversity.SetDefaultValues();

            var response = await _documentDbProvider.CreateDiversityDetailAsync(diversity);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic) response.Resource : null;

        }
    }
}

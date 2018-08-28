using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public class PostDiversityHttpTriggerService : IPostDiversityHttpTriggerService
    {
        public bool DoesDiversityDetailsExistForCustomer(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();

            var doesDiversityDetailsExistForCustomer = documentDbProvider.DoesDiversityDetailsExistForCustomer(customerId);

            return doesDiversityDetailsExistForCustomer;
        }

        public async Task<Models.Diversity> CreateAsync(Models.Diversity diversity)
        {
            if (diversity == null)
                return null;

            diversity.SetDefaultValues();

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.CreateDiversityDetailAsync(diversity);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic) response.Resource : null;

        }
    }
}

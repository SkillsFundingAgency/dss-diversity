using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service
{
    public class PatchDiversityHttpTriggerService : IPatchDiversityHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;
        
        public PatchDiversityHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Diversity> UpdateDiversityAsync(Models.Diversity diversity, DiversityPatch diversityPatch)
        {
            if (diversity == null)
                return null;

            diversityPatch.SetDefaultValues();

            diversity.Patch(diversityPatch);

            var response = await _documentDbProvider.UpdateDiversityDetailAsync(diversity);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? diversity : null;
        }

        public async Task<Models.Diversity> GetDiversityByIdAsync(Guid customerId, Guid diversityId)
        {
            return await _documentDbProvider.GetDiversityDetailForCustomerAsync(customerId, diversityId);
        }
    }
}

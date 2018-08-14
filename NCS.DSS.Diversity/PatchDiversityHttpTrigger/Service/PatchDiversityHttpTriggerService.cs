using System;
using System.Net;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service
{
    public class PatchDiversityHttpTriggerService : IPatchDiversityHttpTriggerService
    {
        public async Task<Models.Diversity> UpdateDiversityAsync(Models.Diversity diversity, DiversityPatch diversityPatch)
        {
            if (diversity == null)
                return null;

            diversityPatch.SetDefaultValues();

            diversity.Patch(diversityPatch);

            var documentDbProvider = new DocumentDBProvider();
            var response = await documentDbProvider.UpdateDiversityDetailAsync(diversity);

            var responseStatusCode = response.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? diversity : null;
        }

        public async Task<Models.Diversity> GetDiversityByIdAsync(Guid customerId, Guid diversityId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var diversityDetail = await documentDbProvider.GetDiversityDetailForCustomerAsync(customerId, diversityId);

            return diversityDetail;
        }
    }
}

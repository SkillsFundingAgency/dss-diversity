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
        private readonly IDiversityPatchService _diversityPatchService;

        public PatchDiversityHttpTriggerService(IDocumentDBProvider documentDbProvider, IDiversityPatchService diversityPatchService)
        {
            _documentDbProvider = documentDbProvider;
            _diversityPatchService = diversityPatchService;
        }

        public string PatchResource(string diversityJson, DiversityPatch diversityPatch)
        {
            if (string.IsNullOrEmpty(diversityJson))
                return null;

            if (diversityPatch == null)
                return null;

            diversityPatch.SetDefaultValues();

            var updatedDiversity = _diversityPatchService.Patch(diversityJson, diversityPatch);

            return updatedDiversity;
        }

        public async Task<Models.Diversity> UpdateCosmosAsync(string diversityJson, Guid diversityId)
        {
            if (string.IsNullOrEmpty(diversityJson))
                return null;

            var response = await _documentDbProvider.UpdateDiversityDetailAsync(diversityJson, diversityId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic) response.Resource : null;
        }

        public async Task<string> GetDiversityForCustomerAsync(Guid customerId, Guid diversityId)
        {
            return await _documentDbProvider.GetDiversityDetailForCustomerToUpdateAsync(customerId, diversityId);
        }

    }
}

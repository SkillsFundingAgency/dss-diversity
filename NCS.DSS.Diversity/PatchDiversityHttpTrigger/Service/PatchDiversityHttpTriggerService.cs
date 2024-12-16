using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.ServiceBus;
using System.Net;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service
{
    public class PatchDiversityHttpTriggerService : IPatchDiversityHttpTriggerService
    {

        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IDiversityPatchService _diversityPatchService;
        private readonly IDiversityServiceBusClient _serviceBusClient;

        public PatchDiversityHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IDiversityPatchService diversityPatchService, IDiversityServiceBusClient serviceBusClient)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _diversityPatchService = diversityPatchService;
            _serviceBusClient = serviceBusClient;
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

            var response = await _cosmosDbProvider.UpdateDiversityDetailAsync(diversityJson, diversityId);

            var responseStatusCode = response?.StatusCode;

            return responseStatusCode == HttpStatusCode.OK ? (dynamic)response.Resource : null;
        }

        public async Task<string> GetDiversityForCustomerAsync(Guid customerId, Guid diversityId)
        {
            return await _cosmosDbProvider.GetDiversityDetailForCustomerToUpdateAsync(customerId, diversityId);
        }

        public async Task SendToServiceBusQueueAsync(DiversityPatch diversityPatch, Guid customerId, string reqUrl)
        {
            await _serviceBusClient.SendPatchMessageAsync(diversityPatch, customerId, reqUrl);
        }


    }
}

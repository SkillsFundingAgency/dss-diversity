using Microsoft.Extensions.Logging;
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
        private readonly IDiversityServiceBusClient _diversityServiceBusClient;
        private readonly ILogger<PatchDiversityHttpTriggerService> _logger;

        public PatchDiversityHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IDiversityPatchService diversityPatchService, IDiversityServiceBusClient diversityServiceBusClient, ILogger<PatchDiversityHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _diversityPatchService = diversityPatchService;
            _diversityServiceBusClient = diversityServiceBusClient;
            _logger = logger;
        }

        public string PatchResource(string diversityJson, DiversityPatch diversityPatch)
        {
            if (string.IsNullOrEmpty(diversityJson))
            {
                _logger.LogInformation("Can't patch diversity because input diversity json is either empty or null");
                return null;
            }

            if (diversityPatch == null)
            {
                _logger.LogInformation("Can't patch diversity because input diversityPatch object is null");
                return null;
            }

            diversityPatch.SetDefaultValues();

            var updatedDiversity = _diversityPatchService.Patch(diversityJson, diversityPatch);

            _logger.LogInformation("Completed patching diversity");

            return updatedDiversity;
        }

        public async Task<Models.Diversity> UpdateCosmosAsync(string diversityJson, Guid diversityId)
        {
            if (string.IsNullOrEmpty(diversityJson))
            {
                _logger.LogInformation("The diversity object provided is either null or empty.");
                return null;
            }

            _logger.LogInformation("Started updating diversity in Cosmos DB with ID: {DiversityId}", diversityId);
            var response = await _cosmosDbProvider.UpdateDiversityDetailAsync(diversityJson, diversityId);

            if (response?.StatusCode == HttpStatusCode.OK)
            {
                _logger.LogInformation("Completed updating diversity in Cosmos DB with ID: {DiversityId}", diversityId);
                return response.Resource;
            }

            _logger.LogError("Failed to update diversity in Cosmos DB with ID: {DiversityId}", diversityId);
            return null;
        }

        public async Task<string> GetDiversityForCustomerAsync(Guid customerId, Guid diversityId)
        {
            return await _cosmosDbProvider.GetDiversityDetailForCustomerToUpdateAsync(customerId, diversityId);
        }

        public async Task SendToServiceBusQueueAsync(DiversityPatch diversityPatch, Guid customerId, string reqUrl)
        {
            try
            {
                _logger.LogInformation("Sending diversity to Service Bus for customer ID: {CustomerId}.", customerId);

                await _diversityServiceBusClient.SendPatchMessageAsync(diversityPatch, customerId, reqUrl);

                _logger.LogInformation("Successfully sent diversity to Service Bus for customer ID: {CustomerId}.", customerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending diversity to Service Bus for customer ID: {CustomerId}.", customerId);
                throw;
            }
        }


    }
}

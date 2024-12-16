using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.ServiceBus;
using System.Net;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public class PostDiversityHttpTriggerService : IPostDiversityHttpTriggerService
    {

        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IDiversityServiceBusClient _serviceBusClient;
        private readonly ILogger<PostDiversityHttpTriggerService> _logger;

        public PostDiversityHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IDiversityServiceBusClient serviceBusClient, ILogger<PostDiversityHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _serviceBusClient = serviceBusClient;
            _logger = logger;
        }

        public async Task<bool> DoesDiversityDetailsExistForCustomer(Guid customerId)
        {
            return await _cosmosDbProvider.DoesDiversityDetailsExistForCustomer(customerId);
        }

        public async Task<Models.Diversity> CreateAsync(Models.Diversity diversity)
        {
            if (diversity == null)
            {
                _logger.LogInformation("Diversity record can't be created because input diversity object is null");
                return null;
            }

            _logger.LogInformation("Started creating diversity in Cosmos DB with ID: {DiversityId}", diversity.DiversityId);

            var response = await _cosmosDbProvider.CreateDiversityDetailAsync(diversity);

            if (response?.StatusCode == HttpStatusCode.Created)
            {
                _logger.LogInformation("Completed creating diversity in Cosmos DB with ID: {DiversityId}", diversity.DiversityId);
                return response.Resource;
            }

            _logger.LogError("Failed to create diversity with ID: {DiversityId}", diversity.DiversityId);
            return null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Diversity diversity, string reqUrl, Guid correlationId, ILogger log)
        {
            try
            {
                _logger.LogInformation("Sending newly created diversity with ID: {DiversityId} to Service Bus for customer ID: {CustomerId}.", diversity.DiversityId, diversity.CustomerId);

                await _serviceBusClient.SendPostMessageAsync(diversity, reqUrl, correlationId, log);

                _logger.LogInformation("Successfully sent diversity with ID: {DiversityId} to Service Bus for customer ID: {CustomerId}.", diversity.DiversityId, diversity.CustomerId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while sending diversity with ID: {DiversityId} to Service Bus for customer ID: {CustomerId}.", diversity.DiversityId, diversity.CustomerId);
                throw;
            }
        }

    }
}

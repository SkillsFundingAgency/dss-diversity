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

        public PostDiversityHttpTriggerService(ICosmosDbProvider cosmosDbProvider, IDiversityServiceBusClient serviceBusClient)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public async Task<bool> DoesDiversityDetailsExistForCustomer(Guid customerId)
        {
            return await _cosmosDbProvider.DoesDiversityDetailsExistForCustomer(customerId);
        }

        public async Task<Models.Diversity> CreateAsync(Models.Diversity diversity)
        {
            if (diversity == null)
                return null;

            var response = await _cosmosDbProvider.CreateDiversityDetailAsync(diversity);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic)response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Diversity diversity, string reqUrl, Guid correlationId, ILogger log)
        {
            await _serviceBusClient.SendPostMessageAsync(diversity, reqUrl, correlationId, log);
        }

    }
}

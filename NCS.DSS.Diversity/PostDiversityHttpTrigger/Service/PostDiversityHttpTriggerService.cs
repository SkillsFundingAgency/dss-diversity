using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Provider;
using NCS.DSS.Diversity.ServiceBus;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public class PostDiversityHttpTriggerService : IPostDiversityHttpTriggerService
    {

        private readonly IDocumentDBProvider _documentDbProvider;
        private readonly IServiceBusClient _serviceBusClient;

        public PostDiversityHttpTriggerService(IDocumentDBProvider documentDbProvider, IServiceBusClient serviceBusClient)
        {
            _documentDbProvider = documentDbProvider;
            _serviceBusClient = serviceBusClient;
        }

        public bool DoesDiversityDetailsExistForCustomer(Guid customerId)
        {
            return _documentDbProvider.DoesDiversityDetailsExistForCustomer(customerId);
        }

        public async Task<Models.Diversity> CreateAsync(Models.Diversity diversity)
        {
            if (diversity == null)
                return null;

            var response = await _documentDbProvider.CreateDiversityDetailAsync(diversity);

            return response.StatusCode == HttpStatusCode.Created ? (dynamic) response.Resource : null;
        }

        public async Task SendToServiceBusQueueAsync(Models.Diversity diversity, string reqUrl, ILogger log)
        {
            await _serviceBusClient.SendPostMessageAsync(diversity, reqUrl, log);
        }

    }
}

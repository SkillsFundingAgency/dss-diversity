using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Diversity.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Diversity.ServiceBus
{

    public partial class DiversityServiceBusClient : IDiversityServiceBusClient
    {
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ILogger<DiversityServiceBusClient> _logger;
        private readonly string _queueName;

        public DiversityServiceBusClient(ServiceBusClient serviceBusClient,
            IOptions<DiversityConfigurationSettings> configOptions,
            ILogger<DiversityServiceBusClient> logger)
        {
            var config = configOptions.Value;
            if (string.IsNullOrEmpty(config.QueueName))
            {
                throw new ArgumentNullException(nameof(config.QueueName), "QueueName cannot be null or empty.");
            }

            _serviceBusClient = serviceBusClient;
            _queueName = config.QueueName;
            _logger = logger;
        }

        public async Task SendPostMessageAsync(Models.Diversity diversity, string reqUrl, Guid correlationId)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Diversity record {" + diversity.DiversityId + "} added for {" + diversity.CustomerId + "} at " + DateTime.UtcNow,
                CustomerGuid = diversity.CustomerId,
                LastModifiedDate = diversity.LastModifiedDate,
                URL = reqUrl + "/" + diversity.DiversityId,
                IsNewCustomer = false,
                TouchpointId = diversity.LastModifiedBy
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = diversity.CustomerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send POST message to service bus. Diversity ID: {DiversityId}", diversity.DiversityId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent POST message to the service bus. Diversity ID: {DiversityId}", diversity.DiversityId);
        }

        public async Task SendPatchMessageAsync(DiversityPatch diversityPatch, Guid customerId, string reqUrl)
        {
            var serviceBusSender = _serviceBusClient.CreateSender(_queueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Diversity record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = diversityPatch.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = diversityPatch.LastModifiedBy
            };

            var msg = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to send PATCH message to service bus. Customer ID: {CustomerId}", customerId);

            await serviceBusSender.SendMessageAsync(msg);

            _logger.LogInformation("Successfully sent PATCH message to the service bus. Customer ID: {CustomerId}", customerId);

        }
    }
}


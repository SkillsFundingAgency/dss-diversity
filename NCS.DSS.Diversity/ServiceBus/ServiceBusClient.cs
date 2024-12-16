﻿using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Models;
using Newtonsoft.Json;
using System.Text;

namespace NCS.DSS.Diversity.ServiceBus
{

    public class ServiceBusClient : IServiceBusClient
    {
        public readonly string QueueName = Environment.GetEnvironmentVariable("QueueName");
        public readonly string ServiceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");        

        public async Task SendPostMessageAsync(Models.Diversity diversity, string reqUrl, Guid correlationId, ILogger log)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel()
            {
                TitleMessage = "New Diversity record {" + diversity.DiversityId + "} added for {" + diversity.CustomerId + "} at " + DateTime.UtcNow,
                CustomerGuid = diversity.CustomerId,
                LastModifiedDate = diversity.LastModifiedDate,
                URL = reqUrl + "/" + diversity.DiversityId,
                IsNewCustomer = false,
                TouchpointId = diversity.LastModifiedBy
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = diversity.CustomerId + " " + DateTime.UtcNow
            };
            //Lof messages to app insights
            //_loggerHelper.LogInformationObject(log, correlationId, string.Format("New Diversity record {0}", diversity.DiversityId), messageModel);

            //await queueClient.SendAsync(msg);
        }

        public async Task SendPatchMessageAsync(DiversityPatch diversityPatch, Guid customerId, string reqUrl)
        {
            var queueClient = new QueueClient(ServiceBusConnectionString, QueueName);

            var messageModel = new MessageModel
            {
                TitleMessage = "Diversity record modification for {" + customerId + "} at " + DateTime.UtcNow,
                CustomerGuid = customerId,
                LastModifiedDate = diversityPatch.LastModifiedDate,
                URL = reqUrl,
                IsNewCustomer = false,
                TouchpointId = diversityPatch.LastModifiedBy
            };

            var msg = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageModel)))
            {
                ContentType = "application/json",
                MessageId = customerId + " " + DateTime.UtcNow
            };

            //await queueClient.SendAsync(msg);

        }

        public class MessageModel
        {
            public string TitleMessage { get; set; }
            public Guid? CustomerGuid { get; set; }
            public DateTime? LastModifiedDate { get; set; }
            public string URL { get; set; }
            public bool IsNewCustomer { get; set; }
            public string TouchpointId { get; set; }
        }

    }
}


﻿using Microsoft.Extensions.Logging;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public interface IPostDiversityHttpTriggerService
    {
        bool DoesDiversityDetailsExistForCustomer(Guid customerId);
        Task<Models.Diversity> CreateAsync(Models.Diversity diversity);
        Task SendToServiceBusQueueAsync(Models.Diversity diversity, string reqUrl, Guid correlationId, ILogger log);
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.ServiceBus
{
    public interface IServiceBusClient
    {
        Task SendPostMessageAsync(Models.Diversity diversity, string reqUrl, ILogger log);
        Task SendPatchMessageAsync(DiversityPatch diversityPatch, Guid customerId, string reqUrl);
    }
}

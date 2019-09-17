using System;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.ServiceBus
{
    public interface IServiceBusClient
    {
        Task SendPostMessageAsync(Models.Diversity diversity, string reqUrl);
        Task SendPatchMessageAsync(DiversityPatch diversityPatch, Guid customerId, string reqUrl);
    }
}

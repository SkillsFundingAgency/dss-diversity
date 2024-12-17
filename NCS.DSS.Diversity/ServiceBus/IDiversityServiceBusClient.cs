using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.ServiceBus
{
    public interface IDiversityServiceBusClient
    {
        Task SendPostMessageAsync(Models.Diversity diversity, string reqUrl, Guid correlationId);
        Task SendPatchMessageAsync(DiversityPatch diversityPatch, Guid customerId, string reqUrl);
    }
}

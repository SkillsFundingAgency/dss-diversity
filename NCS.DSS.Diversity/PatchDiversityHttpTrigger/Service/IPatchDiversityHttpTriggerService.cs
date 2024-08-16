using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service
{
    public interface IPatchDiversityHttpTriggerService
    {
        string PatchResource(string diversityJson, DiversityPatch diversityPatch);
        Task<Models.Diversity> UpdateCosmosAsync(string diversityJson, Guid diversityId);
        Task<string> GetDiversityForCustomerAsync(Guid customerId, Guid diversityId);
        Task SendToServiceBusQueueAsync(DiversityPatch diversityPatch, Guid customerId, string reqUrl);
    }
}

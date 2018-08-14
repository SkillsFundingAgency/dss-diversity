using System;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service
{
    public interface IPatchDiversityHttpTriggerService
    {
        Task<Models.Diversity> UpdateDiversityAsync(Models.Diversity diversity, Models.DiversityPatch diversityPatch);
        Task<Models.Diversity> GetDiversityByIdAsync(Guid customerId, Guid diversityId);
    }
}

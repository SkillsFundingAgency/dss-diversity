using System;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Service
{
    public interface IPostDiversityHttpTriggerService
    {
        bool DoesDiversityDetailsExistForCustomer(Guid customerId);
        Task<Models.Diversity> CreateAsync(Models.Diversity diversity);
    }
}
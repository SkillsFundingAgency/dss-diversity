using System;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Service
{
    public interface IGetDiversityHttpTriggerService
    {
        Task<Guid?> GetDiversityDetailIdAsync(Guid customerId);
    }
}
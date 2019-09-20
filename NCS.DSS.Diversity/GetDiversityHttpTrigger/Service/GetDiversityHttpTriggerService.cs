using System;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Service
{
    public class GetDiversityHttpTriggerService : IGetDiversityHttpTriggerService
    {
        public async Task<Guid?> GetDiversityDetailIdAsync(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var diversityDetailId = await documentDbProvider.GetDiversityDetailIdForCustomerAsync(customerId);

            return diversityDetailId;
        }
    }
}
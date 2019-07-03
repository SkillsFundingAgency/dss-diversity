using System;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Service
{
    public class GetDiversityHttpTriggerService : IGetDiversityHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public GetDiversityHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Guid?> GetDiversityDetailIdAsync(Guid customerId)
        {
            return await _documentDbProvider.GetDiversityDetailIdForCustomerAsync(customerId);
        }
    }
}
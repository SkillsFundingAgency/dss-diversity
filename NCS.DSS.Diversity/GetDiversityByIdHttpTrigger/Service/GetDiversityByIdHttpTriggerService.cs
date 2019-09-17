using System;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service
{
    public class GetDiversityByIdHttpTriggerService : IGetDiversityByIdHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public GetDiversityByIdHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<Models.Diversity> GetDiversityDetailByIdAsync(Guid customerId, Guid diversityId)
        {
            return await _documentDbProvider.GetDiversityDetailIdForCustomerAsync(customerId, diversityId);
        }
    }
}
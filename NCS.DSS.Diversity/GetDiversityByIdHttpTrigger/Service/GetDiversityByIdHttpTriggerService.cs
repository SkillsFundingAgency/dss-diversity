using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service
{
    public class GetDiversityByIdHttpTriggerService : IGetDiversityByIdHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;

        public GetDiversityByIdHttpTriggerService(ICosmosDbProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<Models.Diversity> GetDiversityDetailByIdAsync(Guid customerId, Guid diversityId)
        {
            return await _cosmosDbProvider.GetDiversityDetailForCustomerAsync(customerId, diversityId);
        }
    }
}
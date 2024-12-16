using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Service
{
    public class GetDiversityHttpTriggerService : IGetDiversityHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;

        public GetDiversityHttpTriggerService(ICosmosDbProvider cosmosDbProvider)
        {
            _cosmosDbProvider = cosmosDbProvider;
        }

        public async Task<List<Models.Diversity>> GetDiversityDetailForCustomerAsync(Guid customerId)
        {
            return await _cosmosDbProvider.GetDiversityDetailsForCustomerAsync(customerId);
        }
    }
}
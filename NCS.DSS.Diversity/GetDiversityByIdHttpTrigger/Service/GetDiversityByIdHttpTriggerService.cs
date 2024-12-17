using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.GetDiversityByIdHttpTrigger.Service
{
    public class GetDiversityByIdHttpTriggerService : IGetDiversityByIdHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly ILogger<GetDiversityByIdHttpTriggerService> _logger;

        public GetDiversityByIdHttpTriggerService(ICosmosDbProvider cosmosDbProvider, ILogger<GetDiversityByIdHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<Models.Diversity> GetDiversityDetailByIdAsync(Guid customerId, Guid diversityId)
        {
            _logger.LogInformation("Retrieving diversity with ID: {DiversityId} for customer ID: {CustomerId}.", diversityId, customerId);

            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            if (diversityId == Guid.Empty)
            {
                _logger.LogWarning("Invalid diversity ID provided: {DiversityId}.", diversityId);
                return null;
            }

            var diversity = await _cosmosDbProvider.GetDiversityDetailForCustomerAsync(customerId, diversityId);

            if (diversity == null)
            {
                _logger.LogInformation("No diversity record found with ID: {DiversityId} for customer ID: {CustomerId}", diversityId, customerId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved diversity with ID: {DiversityId} for customer ID: {CustomerId}", diversityId, customerId);
            }

            return diversity;
        }
    }
}
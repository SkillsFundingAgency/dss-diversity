using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Service
{
    public class GetDiversityHttpTriggerService : IGetDiversityHttpTriggerService
    {
        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly ILogger<GetDiversityHttpTriggerService> _logger;

        public GetDiversityHttpTriggerService(ICosmosDbProvider cosmosDbProvider, ILogger<GetDiversityHttpTriggerService> logger)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _logger = logger;
        }

        public async Task<List<Models.Diversity>> GetDiversityDetailForCustomerAsync(Guid customerId)
        {
            _logger.LogTrace("Retrieving diversity record for customer ID: {CustomerId}.", customerId);

            if (customerId == Guid.Empty)
            {
                _logger.LogInformation("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            var diversityRecords = await _cosmosDbProvider.GetDiversityDetailsForCustomerAsync(customerId);

            if (diversityRecords.Count == 0)
            {
                _logger.LogTrace("No diversity record found for customer ID: {CustomerId}", customerId);
            }
            else
            {
                _logger.LogTrace("Successfully retrieved diversity records for customer ID: {CustomerId}", customerId);
            }

            return diversityRecords;
        }
    }
}
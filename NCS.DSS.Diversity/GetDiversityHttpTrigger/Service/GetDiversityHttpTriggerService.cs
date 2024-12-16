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
            _logger.LogInformation("Retrieving diversity record for customer ID: {CustomerId}.", customerId);

            if (customerId == Guid.Empty)
            {
                _logger.LogWarning("Invalid customer ID provided: {CustomerId}.", customerId);
                return null;
            }

            var diversitRecords = await _cosmosDbProvider.GetDiversityDetailsForCustomerAsync(customerId);

            if (diversitRecords == null)
            {
                _logger.LogInformation("No diversity record found for customer ID: {CustomerId}", customerId);
            }
            else
            {
                _logger.LogInformation("Successfully retrieved diversity records for customer ID: {CustomerId}", customerId);
            }

            return diversitRecords;
        }
    }
}
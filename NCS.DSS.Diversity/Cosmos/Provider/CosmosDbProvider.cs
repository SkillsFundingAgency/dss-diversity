using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NCS.DSS.Diversity.Models;
using Newtonsoft.Json;

namespace NCS.DSS.Diversity.Cosmos.Provider
{
    public class CosmosDbProvider : ICosmosDbProvider
    {
        private string _customerJson;
        private readonly Container _diversityContainer;
        private readonly Container _customerContainer;
        private readonly ILogger<CosmosDbProvider> _logger;

        public CosmosDbProvider(CosmosClient cosmosClient,
            IOptions<DiversityConfigurationSettings> configOptions,
            ILogger<CosmosDbProvider> logger)
        {
            var config = configOptions.Value;

            _diversityContainer = GetContainer(cosmosClient, config.DatabaseId, config.CollectionId);
            _customerContainer = GetContainer(cosmosClient, config.CustomerDatabaseId, config.CustomerCollectionId);
            _logger = logger;
        }

        private static Container GetContainer(CosmosClient cosmosClient, string databaseId, string collectionId)
            => cosmosClient.GetContainer(databaseId, collectionId);

        public string GetCustomerJson()
        {
            return _customerJson;
        }

        public async Task<bool> DoesCustomerResourceExist(Guid customerId)
        {
            try
            {
                _logger.LogInformation("Checking for customer resource. Customer ID: {CustomerId}", customerId);

                var response = await _customerContainer.ReadItemAsync<Customer>(
                    customerId.ToString(),
                    PartitionKey.None);

                if (response.Resource != null)
                {
                    _logger.LogInformation("Customer exists. Customer ID: {CustomerId}", customerId);
                    _customerJson = JsonConvert.SerializeObject(response.Resource);
                    return true;
                }

                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogInformation("Customer does not exist. Customer ID: {CustomerId}", customerId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking customer resource existence. Customer ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<bool> DoesDiversityDetailsExistForCustomer(Guid customerId)
        {
            var diversityDetails = await GetDiversityDetailsForCustomerAsync(customerId);

            return diversityDetails.Any();
        }

        public async Task<Models.Diversity> GetDiversityDetailForCustomerAsync(Guid customerId, Guid diversityId)
        {
            _logger.LogInformation("Retrieving Diversity for Customer. Customer ID: {CustomerId}. Diversity ID: {DiversityId}.", customerId, diversityId);

            try
            {
                var query = _diversityContainer.GetItemLinqQueryable<Models.Diversity>()
                    .Where(x => x.CustomerId == customerId && x.DiversityId == diversityId)
                    .ToFeedIterator();

                var response = await query.ReadNextAsync();
                if (response.Any())
                {
                    _logger.LogInformation("Diversity retrieved successfully. Customer ID: {CustomerId}. Diversity ID: {DiversityId}.", customerId, diversityId);
                    return response?.FirstOrDefault();
                }

                _logger.LogWarning("Diversity not found. Customer ID: {CustomerId}. Diversity ID: {DiversityId}.", customerId, diversityId);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving Diversity. Customer ID: {CustomerId}. Diversity ID: {DiversityId}.", customerId, diversityId);
                throw;
            }
        }

        public async Task<List<Models.Diversity>> GetDiversityDetailsForCustomerAsync(Guid customerId)
        {
            _logger.LogInformation("Retrieving Diversities for Customer. Customer ID: {CustomerId}.", customerId);

            try
            {
                var diversities = new List<Models.Diversity>();
                var query = _diversityContainer.GetItemLinqQueryable<Models.Diversity>()
                    .Where(x => x.CustomerId == customerId)
                    .ToFeedIterator();

                while (query.HasMoreResults)
                {
                    var response = await query.ReadNextAsync();
                    diversities.AddRange(response);
                }

                _logger.LogInformation("Retrieved {Count} Diversity record(s) for Customer with ID: {CustomerId}.", diversities.Count, customerId);
                return diversities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving Diversity records. Customer ID: {CustomerId}.", customerId);
                throw;
            }
        }

        public async Task<string> GetDiversityDetailForCustomerToUpdateAsync(Guid customerId, Guid diversityId)
        {
            var diversity = await GetDiversityDetailForCustomerAsync(customerId, diversityId);

            if (diversity == null)
            {
                return string.Empty;
            }

            return JsonConvert.SerializeObject(diversity);
        }

        public async Task<ItemResponse<Models.Diversity>> CreateDiversityDetailAsync(Models.Diversity diversity)
        {
            if (diversity == null)
            {
                _logger.LogError("Diversity object is null. Creation aborted.");
                throw new ArgumentNullException(nameof(diversity), "Diversity cannot be null.");
            }

            _logger.LogInformation("Creating Diversity with ID: {DiversityId}", diversity.DiversityId);

            try
            {
                var response = await _diversityContainer.CreateItemAsync(diversity, PartitionKey.None);
                _logger.LogInformation("Successfully created Diversity with ID: {DiversityID}", diversity.DiversityId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create Diversity with ID: {DiversityId}", diversity.DiversityId);
                throw;
            }
        }

        public async Task<ItemResponse<Models.Diversity>> UpdateDiversityDetailAsync(string diversityJson, Guid diversityId)
        {
            if (string.IsNullOrEmpty(diversityJson))
            {
                _logger.LogError("diversityJson object is null. Update aborted.");
                throw new ArgumentNullException(nameof(diversityJson), "Diversity cannot be null.");
            }

            var diversity = JsonConvert.DeserializeObject<Models.Diversity>(diversityJson);

            _logger.LogInformation("Updating Diversity with ID: {DiversityId}", diversityId);

            try
            {
                var response = await _diversityContainer.ReplaceItemAsync(diversity, diversityId.ToString());
                _logger.LogInformation("Successfully updated Diversity with ID: {DiversityId}", diversityId);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update Diversity with ID: {DiversityId}", diversityId);
                throw;
            }
        }
    }

}
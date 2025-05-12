using DFC.JSON.Standard;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {

        private readonly ICosmosDbProvider _cosmosDbProvider;
        private readonly IJsonHelper _jsonHelper;

        public ResourceHelper(ICosmosDbProvider cosmosDbProvider, IJsonHelper jsonHelper)
        {
            _cosmosDbProvider = cosmosDbProvider;
            _jsonHelper = jsonHelper;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _cosmosDbProvider.DoesCustomerResourceExist(customerId);
        }

        public bool IsCustomerReadOnly()
        {
            var customerJson = _cosmosDbProvider.GetCustomerJson();

            if (string.IsNullOrWhiteSpace(customerJson))
                return false;

            var dateOfTermination = _jsonHelper.GetValue(customerJson, "DateOfTermination");

            return !string.IsNullOrWhiteSpace(dateOfTermination);
        }

    }
}

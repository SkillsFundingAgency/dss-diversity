using Microsoft.Azure.Cosmos;

namespace NCS.DSS.Diversity.Cosmos.Provider
{
    public interface ICosmosDbProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        string GetCustomerJson();
        Task<bool> DoesDiversityDetailsExistForCustomer(Guid customerId);
        Task<Models.Diversity> GetDiversityDetailForCustomerAsync(Guid customerId, Guid diversityId);
        Task<List<Models.Diversity>> GetDiversityDetailsForCustomerAsync(Guid customerId);
        Task<string> GetDiversityDetailForCustomerToUpdateAsync(Guid customerId, Guid diversityId);
        Task<ItemResponse<Models.Diversity>> CreateDiversityDetailAsync(Models.Diversity diversity);
        Task<ItemResponse<Models.Diversity>> UpdateDiversityDetailAsync(string diversityJson, Guid diversityId);
    }
}
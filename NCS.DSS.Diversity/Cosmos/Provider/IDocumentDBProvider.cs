using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Diversity.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        string GetCustomerJson();
        bool DoesDiversityDetailsExistForCustomer(Guid customerId);
        Task<Models.Diversity> GetDiversityDetailIdForCustomerAsync(Guid customerId, Guid diversityId);
        Task<List<Models.Diversity>> GetDiversityDetailsForCustomerAsync(Guid customerId);
        Task<string> GetDiversityDetailForCustomerToUpdateAsync(Guid customerId, Guid diversityId);
        Task<ResourceResponse<Document>> CreateDiversityDetailAsync(Models.Diversity diversity);
        Task<ResourceResponse<Document>> UpdateDiversityDetailAsync(string diversityJson, Guid diversityId);
    }
}
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Diversity.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        Task<bool> DoesCustomerResourceExist(Guid customerId);
        Task<bool> DoesCustomerHaveATerminationDate(Guid customerId);
        bool DoesDiversityDetailsExistForCustomer(Guid customerId);
        Task<Guid?> GetDiversityDetailIdForCustomerAsync(Guid customerId);
        Task<string> GetDiversityDetailForCustomerToUpdateAsync(Guid customerId, Guid diversityId);
        Task<Models.Diversity> GetDiversityDetailForCustomerAsync(Guid customerId, Guid diversityId);
        Task<ResourceResponse<Document>> CreateDiversityDetailAsync(Models.Diversity diversity);
        Task<ResourceResponse<Document>> UpdateDiversityDetailAsync(string diversityJson, Guid diversityId);
    }
}
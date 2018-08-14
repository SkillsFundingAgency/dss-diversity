using System;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Diversity.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        Task<Guid?> GetDiversityDetailIdForCustomerAsync(Guid customerId);
        Task<Models.Diversity> GetDiversityDetailForCustomerAsync(Guid customerId, Guid diversityId);
        Task<ResourceResponse<Document>> CreateDiversityDetailAsync(Models.Diversity diversity);
        Task<ResourceResponse<Document>> UpdateDiversityDetailAsync(Models.Diversity diversity);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace NCS.DSS.Diversity.Cosmos.Provider
{
    public interface IDocumentDBProvider
    {
        bool DoesCustomerResourceExist(Guid customerId);
        List<Models.Diversity> GetDiversityDetailsForCustomer(Guid customerId);
        Task<ResourceResponse<Document>> CreatDiversityDetailAsync(Models.Diversity diversity);
        Task<ResourceResponse<Document>> UpdateDiversityDetailAsync(Models.Diversity diversity);
    }
}
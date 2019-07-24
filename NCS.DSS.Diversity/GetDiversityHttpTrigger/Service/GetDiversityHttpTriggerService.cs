using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Service
{
    public class GetDiversityHttpTriggerService : IGetDiversityHttpTriggerService
    {
        private readonly IDocumentDBProvider _documentDbProvider;

        public GetDiversityHttpTriggerService(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<List<Models.Diversity>> GetDiversityDetailForCustomerAsync(Guid customerId)
        {
            return await _documentDbProvider.GetDiversityDetailsForCustomerAsync(customerId);
        }
    }
}
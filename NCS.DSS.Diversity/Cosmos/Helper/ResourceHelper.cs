using System;
using System.Threading.Tasks;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {

        private readonly IDocumentDBProvider _documentDbProvider;
        public ResourceHelper(IDocumentDBProvider documentDbProvider)
        {
            _documentDbProvider = documentDbProvider;
        }

        public async Task<bool> DoesCustomerExist(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerResourceExist(customerId);
        }

        public async Task<bool> IsCustomerReadOnly(Guid customerId)
        {
            return await _documentDbProvider.DoesCustomerHaveATerminationDate(customerId);
        }

    }
}

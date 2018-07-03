using System;
using NCS.DSS.Diversity.Cosmos.Provider;

namespace NCS.DSS.Diversity.Cosmos.Helper
{
    public class ResourceHelper : IResourceHelper
    {
        public bool DoesCustomerExist(Guid customerId)
        {
            var documentDbProvider = new DocumentDBProvider();
            var doesCustomerExist = documentDbProvider.DoesCustomerResourceExist(customerId);

            return doesCustomerExist;
        }
    }
}

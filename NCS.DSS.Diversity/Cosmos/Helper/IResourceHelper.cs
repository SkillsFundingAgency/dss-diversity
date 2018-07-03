using System;

namespace NCS.DSS.Diversity.Cosmos.Helper
{
    public interface IResourceHelper
    {
        bool DoesCustomerExist(Guid customerId);
    }
}
using Microsoft.AspNetCore.Http;
using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.Helpers
{
    public interface IHelper
    {
        Task UpdateValuesAsync<T>(HttpRequest request, T diversity) where T : IDiversity;
    }
}

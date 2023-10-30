using Microsoft.AspNetCore.Http;
using NCS.DSS.Diversity.Models;

namespace NCS.DSS.Diversity.Helpers
{
    public interface IHelper
    {
        void UpdateValues<T>(HttpRequest request, T diversity) where T : IDiversity;
    }
}

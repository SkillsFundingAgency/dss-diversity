using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.Helpers
{
    public interface IHttpRequestMessageHelper
    {
        Task<T> GetDiversityFromRequest<T>(HttpRequestMessage req);
        string GetTouchpointId(HttpRequestMessage req);
    }
}
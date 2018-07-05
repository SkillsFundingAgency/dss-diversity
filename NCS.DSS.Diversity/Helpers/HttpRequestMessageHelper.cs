using System.Net.Http;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<Models.Diversity> GetDiversityFromRequest(HttpRequestMessage req)
        {
            return await req.Content.ReadAsAsync<Models.Diversity>();
        }
    }
}

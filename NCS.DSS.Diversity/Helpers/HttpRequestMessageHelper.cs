using System;
using System.Linq;
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

        public Guid? GetTouchpointId(HttpRequestMessage req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("APIM-TouchpointId"))
                return null;

            var touchpointId = req.Headers.GetValues("APIM-TouchpointId").FirstOrDefault();

            if (!Guid.TryParse(touchpointId, out var touchpountGuid))
                return null;

            return touchpountGuid;
        }
    }
}

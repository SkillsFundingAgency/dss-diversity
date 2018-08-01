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
            if (req == null)
                return default(Models.Diversity);

            if (req.Content?.Headers?.ContentType != null)
                req.Content.Headers.ContentType.MediaType = "application/json";

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

using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NCS.DSS.Diversity.Helpers
{
    public class HttpRequestMessageHelper : IHttpRequestMessageHelper
    {
        public async Task<Models.Diversity> GetDiversityFromRequest(HttpRequestMessage req)
        {
            if (req == null)
                return default(Models.Diversity);

            if (req.Content?.Headers != null)
                req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return await req.Content.ReadAsAsync<Models.Diversity>();
        }

        public string GetTouchpointId(HttpRequestMessage req)
        {
            if (req?.Headers == null)
                return null;

            if (!req.Headers.Contains("TouchpointId"))
                return null;

            var touchpointId = req.Headers.GetValues("TouchpointId").FirstOrDefault();

            return string.IsNullOrEmpty(touchpointId) ? string.Empty : touchpointId;
        }
    }
}

using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.CDS.Diversity.PostDiversityHttpTrigger
{
    public static class PostDiversityHttpTrigger
    {
        [FunctionName("Post")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Diversity")]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }
}
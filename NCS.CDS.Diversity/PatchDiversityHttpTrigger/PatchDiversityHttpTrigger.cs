using System;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.CDS.Diversity.PatchDiversityHttpTrigger
{
    public static class PatchDiversityHttpTrigger
    {
        [FunctionName("Patch")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Diversity/{diversityId:guid}")]HttpRequestMessage req, TraceWriter log, string diversityId)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(diversityId),
                        System.Text.Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(diversityGuid),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}
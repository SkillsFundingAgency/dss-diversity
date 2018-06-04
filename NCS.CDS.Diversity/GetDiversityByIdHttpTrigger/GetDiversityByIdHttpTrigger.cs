using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace NCS.CDS.Diversity.GetDiversityByIdHttpTrigger
{
    public static class GetDiversityByIdHttpTrigger
    {
        [FunctionName("GetById")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Diversity/{diversityId:guid}")]HttpRequestMessage req, TraceWriter log, string diversityId)
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
            var service = new GetDiversityByIdHttpTriggerService();
            var values = await service.GetDiversity(diversityGuid);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(values),
                    System.Text.Encoding.UTF8, "application/json")
            };
        }
    }
}
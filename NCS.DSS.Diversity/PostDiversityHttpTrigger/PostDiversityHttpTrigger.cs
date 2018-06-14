using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger
{
    public static class PostDiversityHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Diversity))]
        [Display(Name = "Post", Description = "Ability to create a new diversity record for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/DiversityDetails")]HttpRequestMessage req, TraceWriter log, string customerId)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // Get request body
            var diversity = await req.Content.ReadAsAsync<Models.Diversity>();

            var diversityService = new PostDiversityHttpTriggerService();
            var diversityId = diversityService.Create(diversity);

            return diversityId == null
                ? new HttpResponseMessage(HttpStatusCode.BadRequest)
                : new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Created Diversity record with Id of : " + diversityId)
                };
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Annotations;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.GetDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Helpers;
using NCS.DSS.Diversity.Ioc;

namespace NCS.DSS.Diversity.GetDiversityHttpTrigger.Function
{
    public static class GetDiversityHttpTrigger
    {
        [FunctionName("Get")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Details found", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity Details do not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Display(Name = "Get", Description = "Ability to return the diversity details for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "Customers/{customerId}/DiversityDetails")] HttpRequestMessage req, ILogger log, string customerId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IGetDiversityHttpTriggerService getDiversityService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
           if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header.");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("C# HTTP trigger function processed a request. " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var diversityId = await getDiversityService.GetDiversityDetailIdAsync(customerGuid);

            return diversityId == null ?
                HttpResponseMessageHelper.NoContent(customerGuid) : 
                HttpResponseMessageHelper.Ok(diversityId.Value);
        }
    }
}

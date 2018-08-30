using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Annotations;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Helpers;
using NCS.DSS.Diversity.Ioc;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Function
{
    public static class PostDiversityHttpTrigger
    {
        [FunctionName("Post")]
        [ResponseType(typeof(Models.Diversity))]
        [Response(HttpStatusCode = (int) HttpStatusCode.Created, Description = "Diversity Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int) HttpStatusCode.NoContent, Description = "Diversity does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int) HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int) HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int) HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Conflict, Description = "Diversity Details already exists for customer", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new diversity record for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/DiversityDetails")] HttpRequestMessage req, ILogger log, string customerId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPostDiversityHttpTriggerService postDiversityService)
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
            
            Models.Diversity diversityRequest;

            try
            {
                diversityRequest = await httpRequestMessageHelper.GetDiversityFromRequest<Models.Diversity>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (diversityRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            diversityRequest.SetIds(customerGuid, touchpointId);

            var errors = validate.ValidateResource(diversityRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var doesDiversityDetailsExist = postDiversityService.DoesDiversityDetailsExistForCustomer(customerGuid);

            if (doesDiversityDetailsExist)
                return HttpResponseMessageHelper.Conflict();

            var diversity = await postDiversityService.CreateAsync(diversityRequest);

            return diversity == null
                ? HttpResponseMessageHelper.BadRequest(customerGuid)
                : HttpResponseMessageHelper.Created(JsonHelper.SerializeObject(diversity));
        }
    }
}
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Description;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
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
        [Response(HttpStatusCode = 422, Description = "Diversity validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new diversity record for a given customer.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/DiversityDetails")]
            HttpRequestMessage req, TraceWriter log, string customerId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPostDiversityHttpTriggerService postDiversityService)
        {
            log.Info("C# HTTP trigger function processed a request.");

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);


            Models.Diversity diversityRequest;

            try
            {
                diversityRequest = await httpRequestMessageHelper.GetDiversityFromRequest(req);
            }
            catch (JsonSerializationException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (diversityRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            var errors = validate.ValidateResource(diversityRequest);

            if (errors != null && errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var diversity = await postDiversityService.CreateAsync(diversityRequest);

            return diversity == null
                ? HttpResponseMessageHelper.BadRequest(customerGuid)
                : HttpResponseMessageHelper.Created(diversity);
        }
    }
}
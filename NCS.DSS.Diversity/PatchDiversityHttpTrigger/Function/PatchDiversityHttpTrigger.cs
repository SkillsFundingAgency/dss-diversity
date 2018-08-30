using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Annotations;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Helpers;
using NCS.DSS.Diversity.Ioc;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Function
{
    public static class PatchDiversityHttpTrigger
    {
        [FunctionName("Patch")]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Detail Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an diversity detail record.")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/DiversityDetails/{diversityId}")]HttpRequestMessage req, ILogger log, string customerId, string diversityId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IHttpRequestMessageHelper httpRequestMessageHelper,
            [Inject]IValidate validate,
            [Inject]IPatchDiversityHttpTriggerService patchDiversityService)
        {
            var touchpointId = httpRequestMessageHelper.GetTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                log.LogInformation("Unable to locate 'TouchpointId' in request header");
                return HttpResponseMessageHelper.BadRequest();
            }

            log.LogInformation("C# HTTP trigger function Patch Customer processed a request. By Touchpoint " + touchpointId);

            if (!Guid.TryParse(customerId, out var customerGuid))
                return HttpResponseMessageHelper.BadRequest(customerGuid);

            if (!Guid.TryParse(diversityId, out var diversityGuid))
                return HttpResponseMessageHelper.BadRequest(diversityGuid);

            Models.DiversityPatch diversityPatchRequest;

            try
            {
                diversityPatchRequest = await httpRequestMessageHelper.GetDiversityFromRequest<Models.DiversityPatch>(req);
            }
            catch (JsonException ex)
            {
                return HttpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (diversityPatchRequest == null)
                return HttpResponseMessageHelper.UnprocessableEntity(req);

            diversityPatchRequest.LastModifiedBy = touchpointId;

            // validate the request
            var errors = validate.ValidateResource(diversityPatchRequest);

            if (errors.Any())
                return HttpResponseMessageHelper.UnprocessableEntity(errors);

            var doesCustomerExist = resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
                return HttpResponseMessageHelper.Forbidden(customerGuid);

            var diversity = await patchDiversityService.GetDiversityByIdAsync(customerGuid, diversityGuid);

            if (diversity == null)
                return HttpResponseMessageHelper.NoContent(customerGuid);

            var updatedDiversity = await patchDiversityService.UpdateDiversityAsync(diversity, diversityPatchRequest);

            return updatedDiversity == null ?
                HttpResponseMessageHelper.BadRequest(customerGuid) :
                HttpResponseMessageHelper.Ok();
        }
    }
}
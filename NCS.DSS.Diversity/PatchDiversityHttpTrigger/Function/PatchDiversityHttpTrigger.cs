using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.Logging;
using DFC.Functions.DI.Standard.Attributes;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
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
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/DiversityDetails/{diversityId}")]HttpRequest req, ILogger log, string customerId, string diversityId,
            [Inject]IResourceHelper resourceHelper,
            [Inject]IPatchDiversityHttpTriggerService patchDiversityService,
            [Inject]IValidate validate,
            [Inject]ILoggerHelper loggerHelper,
            [Inject]IHttpRequestHelper httpRequestHelper,
            [Inject]IHttpResponseMessageHelper httpResponseMessageHelper,
            [Inject]IJsonHelper jsonHelper)
        {
            loggerHelper.LogMethodEnter(log);

            var correlationId = httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Unable to locate 'TouchpointId' in request header");
                return httpResponseMessageHelper.BadRequest();
            }

            loggerHelper.LogInformationMessage(log, correlationGuid,
                string.Format("Get PatchDiversityHttpTrigger C# HTTP trigger function  processed a request. By Touchpoint: {0}",
                    touchpointId));


            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", customerId));
                return httpResponseMessageHelper.BadRequest(customerGuid);
            }

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Unable to parse 'customerId' to a Guid: {0}", diversityGuid));
                return httpResponseMessageHelper.BadRequest(diversityGuid);
            }

            Models.DiversityPatch diversityPatchRequest;

            try
            {
                diversityPatchRequest = await httpRequestHelper.GetResourceFromRequest<Models.DiversityPatch>(req);
            }
            catch (JsonException ex)
            {
                loggerHelper.LogException(log, correlationGuid, "Unable to retrieve body from req", ex);
                return httpResponseMessageHelper.UnprocessableEntity(ex);
            }

            if (diversityPatchRequest == null)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "Diversity patch request is null");
                return httpResponseMessageHelper.UnprocessableEntity(req);
            }

            diversityPatchRequest.LastModifiedBy = touchpointId;

            // validate the request
            loggerHelper.LogInformationMessage(log, correlationGuid, "Attempt to validate resource");
            var errors = validate.ValidateResource(diversityPatchRequest);

            if (errors != null && errors.Any())
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, "validation errors with resource");
                return httpResponseMessageHelper.UnprocessableEntity(errors);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if customer exists {0}", customerGuid));
            var doesCustomerExist = await resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer does not exist {0}", customerGuid));
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Attempting to see if this is a read only customer {0}", customerGuid));
            var isCustomerReadOnly = await resourceHelper.IsCustomerReadOnly(customerGuid);

            if (isCustomerReadOnly)
            {
                loggerHelper.LogInformationMessage(log, correlationGuid, string.Format("Customer is read only {0}", customerGuid));
                return httpResponseMessageHelper.Forbidden(customerGuid);
            }

            var diversity = await patchDiversityService.GetDiversityByIdAsync(customerGuid, diversityGuid);

            if (diversity == null)
            {
                return httpResponseMessageHelper.NoContent(customerGuid);
            }

            var updatedDiversity = await patchDiversityService.UpdateDiversityAsync(diversity, diversityPatchRequest);

            return updatedDiversity == null ?
                httpResponseMessageHelper.BadRequest(customerGuid) :
                httpResponseMessageHelper.Ok();
        }
    }
}
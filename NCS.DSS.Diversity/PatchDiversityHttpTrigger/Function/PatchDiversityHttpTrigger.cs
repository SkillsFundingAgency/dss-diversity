using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DFC.Common.Standard.GuidHelper;
using DFC.Common.Standard.Logging;
using DFC.HTTP.Standard;
using DFC.JSON.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using Newtonsoft.Json;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Function
{
    public class PatchDiversityHttpTrigger
    {

        private readonly IResourceHelper _resourceHelper;
        private readonly IPatchDiversityHttpTriggerService _patchDiversityService;
        private readonly IValidate _validate;
        private readonly ILoggerHelper _loggerHelper;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IHttpResponseMessageHelper _httpResponseMessageHelper;
        private readonly IJsonHelper _jsonHelper;
        private readonly IGuidHelper _guidHelper;

        public PatchDiversityHttpTrigger(IResourceHelper resourceHelper, IPatchDiversityHttpTriggerService patchDiversityService, IValidate validate, ILoggerHelper loggerHelper, IHttpRequestHelper httpRequestHelper, IHttpResponseMessageHelper httpResponseMessageHelper, IJsonHelper jsonHelper, IGuidHelper guidHelper)
        {
            _resourceHelper = resourceHelper;
            _patchDiversityService = patchDiversityService;
            _validate = validate;
            _loggerHelper = loggerHelper;
            _httpRequestHelper = httpRequestHelper;
            _httpResponseMessageHelper = httpResponseMessageHelper;
            _jsonHelper = jsonHelper;
            _guidHelper = guidHelper;
        }


        [FunctionName("Patch")]
        [ProducesResponseType(typeof(Models.Diversity), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Detail Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an diversity detail record.")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/DiversityDetails/{diversityId}")]HttpRequest req, ILogger log, string customerId, string diversityId)
        {
            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
                log.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                log.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }
            log.LogInformation($"DssCorrelationId: {correlationGuid}");

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = _httpResponseMessageHelper.BadRequest();
                log.LogWarning($"Response Status Code: {response.StatusCode}. Unable to locate 'APIM-TouchpointId' in request header");
                return response;
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                var response = _httpResponseMessageHelper.BadRequest();
                log.LogWarning($"Unable to locate 'apimurl' in request header");
                return response;
            }

            log.LogInformation($"PatchDiversityHttpTrigger C# HTTP trigger function  processed a request. By Touchpoint: {touchpointId}");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response =_httpResponseMessageHelper.BadRequest(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}");
                return response;
            }

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                var response = _httpResponseMessageHelper.BadRequest(diversityId);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'diversityId' to a Guid: {diversityId}");
                return response;
            }

            Models.DiversityPatch diversityPatchRequest;

            try
            {
                diversityPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.DiversityPatch>(req);
            }
            catch (JsonException ex)
            {
                var response = _httpResponseMessageHelper.UnprocessableEntity(ex);
                log.LogError($"Response Status Code: {response.StatusCode}. Unable to retrieve body from req", ex);
                return response;
            }

            if (diversityPatchRequest == null)
            {
                var response = _httpResponseMessageHelper.UnprocessableEntity(req);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Diversity patch request is null");
                return response;
            }

            diversityPatchRequest.LastModifiedBy = touchpointId;

            // validate the request
            log.LogInformation($"Attempt to validate resource");
            var errors = _validate.ValidateResource(diversityPatchRequest);

            if (errors != null && errors.Any())
            {
                var response = _httpResponseMessageHelper.UnprocessableEntity(errors);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Validation errors with resource", errors);
                return response;
            }

            log.LogInformation($"Attempting to see if customer exists {customerGuid}");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = _httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Customer does not exist {customerGuid}");
                return response;
            }

            log.LogInformation($"Attempting to see if this is a read only customer {customerGuid}");
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                var response = _httpResponseMessageHelper.Forbidden(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Customer is read only {customerGuid}");
                return response;
            }

            log.LogInformation($"Attempting to get diversity {diversityGuid} for the customer {customerGuid}");
            var diversity = await _patchDiversityService.GetDiversityForCustomerAsync(customerGuid, diversityGuid);

            if (diversity == null)
            {
                var response = _httpResponseMessageHelper.NoContent(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Couldnt find diversity for the customer {customerGuid}.");
                return response;
            }

            var patchedDiversity = _patchDiversityService.PatchResource(diversity, diversityPatchRequest);

            log.LogInformation($"Attempting to patch diversity {diversityGuid}");
            var updatedDiversity = await _patchDiversityService.UpdateCosmosAsync(patchedDiversity, diversityGuid);

            if (updatedDiversity != null)
            {
                log.LogInformation($"attempting to send to service bus {updatedDiversity.DiversityId}");
                await _patchDiversityService.SendToServiceBusQueueAsync(diversityPatchRequest, customerGuid, apimUrl);
            }

            if (updatedDiversity == null)
            {
                var response =_httpResponseMessageHelper.BadRequest(customerGuid);
                log.LogWarning($"Response Status Code: {response.StatusCode}. Patch diversity failed for the customer {customerGuid}.");
                return response;
            }
            else
            {
                var response = _httpResponseMessageHelper.Ok(_jsonHelper.SerializeObjectAndRenameIdProperty(updatedDiversity, "id", "DiversityId"));
                log.LogInformation($"Response Status Code: {response.StatusCode}. Successfully updated diversity {updatedDiversity} for the customer {customerGuid}");
                return response;
            }
        }
    }
}
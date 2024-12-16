using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.PatchDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.Diversity.PatchDiversityHttpTrigger.Function
{
    public class PatchDiversityHttpTrigger
    {

        private readonly IPatchDiversityHttpTriggerService _patchDiversityService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<PatchDiversityHttpTrigger> _logger;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public PatchDiversityHttpTrigger(
            IPatchDiversityHttpTriggerService patchDiversityService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<PatchDiversityHttpTrigger> logger,
            IValidate validate,
            IDynamicHelper dynamicHelper)
        {
            _patchDiversityService = patchDiversityService;
            _httpRequestHelper = httpRequestHelper;
            _resourceHelper = resourceHelper;
            _logger = logger;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
        }


        [Function("Patch")]
        [ProducesResponseType(typeof(Models.Diversity), 200)]
        [Response(HttpStatusCode = (int)HttpStatusCode.OK, Description = "Diversity Detail Updated", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NoContent, Description = "Diversity Detail does not exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity Detail validation error(s)", ShowSchema = false)]
        [Display(Name = "Patch", Description = "Ability to modify/update an diversity detail record.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "Customers/{customerId}/DiversityDetails/{diversityId}")] HttpRequest req, string customerId, string diversityId)
        {
            // Ensure the request body can be read multiple times by enabling buffering
            req.EnableBuffering();

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (string.IsNullOrEmpty(correlationId))
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid");
                correlationGuid = Guid.NewGuid();
            }
            _logger.LogInformation($"DssCorrelationId: {correlationGuid}");

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                var response = new BadRequestResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to locate 'APIM-TouchpointId' in request header");
                return response;
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                var response = new BadRequestResult();
                _logger.LogWarning($"Unable to locate 'apimurl' in request header");
                return response;
            }

            _logger.LogInformation($"PatchDiversityHttpTrigger C# HTTP trigger function  processed a request. By Touchpoint: {touchpointId}");

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                var response = new BadRequestObjectResult(customerGuid.ToString());
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'customerId' to a Guid: {customerId}");
                return response;
            }

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                var response = new BadRequestObjectResult(diversityId);
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Unable to parse 'diversityId' to a Guid: {diversityId}");
                return response;
            }

            Models.DiversityPatch diversityPatchRequest;

            try
            {
                diversityPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.DiversityPatch>(req);

                // Fix for bug AD-157065 (Oct '23)
                if (diversityPatchRequest.ConsentToCollectEthnicity == null)
                    diversityPatchRequest.ConsentToCollectEthnicity = false;

                if (diversityPatchRequest.ConsentToCollectLLDDHealth == null)
                    diversityPatchRequest.ConsentToCollectLLDDHealth = false;
            }
            catch (JsonException ex)
            {
                var response = new UnprocessableEntityObjectResult(_dynamicHelper.ExcludeProperty(ex, PropertyToExclude));
                _logger.LogError($"Response Status Code: {response.StatusCode}. Unable to retrieve body from req", ex);
                return response;
            }

            if (diversityPatchRequest == null)
            {
                var response = new UnprocessableEntityObjectResult(req);
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Diversity patch request is null");
                return response;
            }

            diversityPatchRequest.LastModifiedBy = touchpointId;

            // validate the request
            _logger.LogInformation($"Attempt to validate resource");
            var errors = _validate.ValidateResource(diversityPatchRequest);

            if (errors != null && errors.Any())
            {
                var response = new UnprocessableEntityObjectResult(errors);
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Validation errors with resource", errors);
                return response;
            }

            _logger.LogInformation($"Attempting to see if customer exists {customerGuid}");
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Customer does not exist {customerGuid}");
                return response;
            }

            _logger.LogInformation($"Attempting to see if this is a read only customer {customerGuid}");
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult(customerGuid.ToString())
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                };

                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Customer is read only {customerGuid}");
                return response;
            }

            _logger.LogInformation($"Attempting to get diversity {diversityGuid} for the customer {customerGuid}");
            var diversity = await _patchDiversityService.GetDiversityForCustomerAsync(customerGuid, diversityGuid);

            if (diversity == null)
            {
                var response = new NoContentResult();
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Couldnt find diversity for the customer {customerGuid}.");
                return response;
            }

            var patchedDiversity = _patchDiversityService.PatchResource(diversity, diversityPatchRequest);

            _logger.LogInformation($"Attempting to patch diversity {diversityGuid}");
            var updatedDiversity = await _patchDiversityService.UpdateCosmosAsync(patchedDiversity, diversityGuid);

            if (updatedDiversity != null)
            {
                _logger.LogInformation($"attempting to send to service bus {updatedDiversity.DiversityId}");
                await _patchDiversityService.SendToServiceBusQueueAsync(diversityPatchRequest, customerGuid, apimUrl);
            }

            if (updatedDiversity == null)
            {
                var response = new BadRequestObjectResult(customerGuid.ToString());
                _logger.LogWarning($"Response Status Code: {response.StatusCode}. Patch diversity failed for the customer {customerGuid}.");
                return response;
            }
            else
            {
                var response = new JsonResult(updatedDiversity, new JsonSerializerOptions())
                {
                    StatusCode = (int)HttpStatusCode.OK,
                };

                _logger.LogInformation($"Response Status Code: {response.StatusCode}. Successfully updated diversity {updatedDiversity} for the customer {customerGuid}");
                return response;
            }
        }
    }
}
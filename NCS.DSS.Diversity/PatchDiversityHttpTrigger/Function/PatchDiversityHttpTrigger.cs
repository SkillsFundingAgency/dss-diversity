using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Models;
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
            _logger.LogInformation("Function {FunctionName} has been invoked", nameof(PatchDiversityHttpTrigger));

            // Ensure the request body can be read multiple times by enabling buffering
            req.EnableBuffering();

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);
            if (string.IsNullOrEmpty(correlationId))
            {
                _logger.LogInformation("Unable to locate 'DssCorrelationId' in request header");
            }

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid. CorrelationId: {CorrelationId}", correlationId);
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogWarning("Unable to locate 'TouchpointId' in request header");
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header");
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogWarning("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult($"Unable to locate 'apimURL' in request header. Correlation GUID: {correlationGuid}");
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogWarning("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult($"Unable to parse 'customerId' to a GUID. Customer GUID: {customerId}");
            }

            if (!Guid.TryParse(diversityId, out var diversityGuid))
            {
                _logger.LogWarning("Unable to parse 'diversityId' to a GUID. Diversity GUID: {DiversityID}", diversityId);
                return new BadRequestObjectResult($"Unable to parse 'diversityId' to a GUID. Diversity GUID: {diversityId}");
            }

            _logger.LogInformation("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            Models.DiversityPatch diversityPatchRequest;

            try
            {
                _logger.LogInformation("Attempting to retrieve resource from request. Correlation GUID: {CorrelationGuid}", correlationGuid);
                diversityPatchRequest = await _httpRequestHelper.GetResourceFromRequest<Models.DiversityPatch>(req);

                // Fix for bug AD-157065 (Oct '23)
                if (diversityPatchRequest.ConsentToCollectEthnicity == null)
                    diversityPatchRequest.ConsentToCollectEthnicity = false;

                if (diversityPatchRequest.ConsentToCollectLLDDHealth == null)
                    diversityPatchRequest.ConsentToCollectLLDDHealth = false;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to parse {diversityPatchRequest} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(diversityPatchRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult($"Unable to parse Diversity Details from request body. Exception: {ex.Message}");
            }

            if (diversityPatchRequest == null)
            {
                _logger.LogWarning("{diversityPatchRequest} object is NULL. Correlation GUID: {CorrelationGuid}", nameof(diversityPatchRequest), correlationGuid);
                return new UnprocessableEntityObjectResult($"Diversity Details in request body are NULL. Please supply this data.");
            }

            diversityPatchRequest.LastModifiedBy = touchpointId;

            // validate the request            
            _logger.LogInformation("Attempting to validate {diversityPatchRequest} object", nameof(diversityPatchRequest));
            var errors = _validate.ValidateResource(diversityPatchRequest);

            if (errors != null && errors.Any())
            {
                _logger.LogWarning("Falied to validate {diversityPatchRequest} object", nameof(diversityPatchRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogInformation("Successfully validated {diversityPatchRequest} object", nameof(diversityPatchRequest));


            _logger.LogInformation("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogWarning("Customer not found. Customer ID: {CustomerId}.", customerGuid);
                return new NotFoundObjectResult($"Customer not found. Customer ID: {customerGuid}.");
            }
            _logger.LogInformation("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogInformation("Checking if customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult($"Customer is read-only. Customer GUID: {customerGuid}.")
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                };

                _logger.LogWarning("Customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
                return response;
            }

            _logger.LogInformation("Attempting to get Diversity for Customer. Customer GUID: {CustomerId}. Diversity GUID: {DiversityId}.", customerGuid, diversityGuid);
            var diversity = await _patchDiversityService.GetDiversityForCustomerAsync(customerGuid, diversityGuid);

            if (diversity == null)
            {
                _logger.LogWarning("Diversity not found. Customer GUID: {CustomerId}. Diversity GUID: {DiversityId}.", customerGuid, diversityGuid);
                return new NotFoundObjectResult($"Diversity not found. Customer GUID: {customerGuid}. Diversity GUID: {diversityGuid}.");
            }

            _logger.LogInformation("Attempting to PATCH Diversity resource.");
            var patchedDiversity = _patchDiversityService.PatchResource(diversity, diversityPatchRequest);
            if (patchedDiversity == null)
            {
                _logger.LogWarning("Failed to PATCH Diversity resource.");
                return new BadRequestObjectResult("Failed to PATCH Diversity resource.");
            }

            _logger.LogInformation("Attempting to update Diversity in Cosmos DB. Diversity GUID: {DiversityId}", diversityGuid);
            var updatedDiversity = await _patchDiversityService.UpdateCosmosAsync(patchedDiversity, diversityGuid);
            if (updatedDiversity == null)
            {
                _logger.LogWarning("Failed to update Diversity in Cosmos DB. Diversity GUID: {DiversityId}", diversityGuid);
                _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchDiversityHttpTrigger));
                return new BadRequestObjectResult($"Failed to update Diversity in Cosmos DB. Diversity GUID: {diversityGuid}");
            }
            _logger.LogInformation("Diversity updated successfully in Cosmos DB. Diversity GUID: {DiversityId}", diversityGuid);


            _logger.LogInformation("Attempting to send message to Service Bus Namespace. Diversity GUID: {DiversityId}", diversityGuid);
            await _patchDiversityService.SendToServiceBusQueueAsync(diversityPatchRequest, customerGuid, apimUrl);
            _logger.LogInformation("Successfully sent message to Service Bus. Diversity GUID: {DiversityId}", diversityGuid);


            _logger.LogInformation("Function {FunctionName} has finished invoking", nameof(PatchDiversityHttpTrigger));
            return new JsonResult(updatedDiversity, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.OK,
            };
        }
    }
}
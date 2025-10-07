using DFC.HTTP.Standard;
using DFC.Swagger.Standard.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using NCS.DSS.Diversity.Cosmos.Helper;
using NCS.DSS.Diversity.Models;
using NCS.DSS.Diversity.PostDiversityHttpTrigger.Service;
using NCS.DSS.Diversity.Validation;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using JsonException = Newtonsoft.Json.JsonException;

namespace NCS.DSS.Diversity.PostDiversityHttpTrigger.Function
{
    public class PostDiversityHttpTrigger
    {

        private readonly IPostDiversityHttpTriggerService _postDiversityService;
        private readonly IHttpRequestHelper _httpRequestHelper;
        private readonly IResourceHelper _resourceHelper;
        private readonly ILogger<PostDiversityHttpTrigger> _logger;
        private readonly IValidate _validate;
        private readonly IDynamicHelper _dynamicHelper;
        private static readonly string[] PropertyToExclude = { "TargetSite" };

        public PostDiversityHttpTrigger(
            IPostDiversityHttpTriggerService postDiversityService,
            IHttpRequestHelper httpRequestHelper,
            IResourceHelper resourceHelper,
            ILogger<PostDiversityHttpTrigger> logger,
            IValidate validate,
            IDynamicHelper dynamicHelper)
        {
            _postDiversityService = postDiversityService;
            _httpRequestHelper = httpRequestHelper;
            _logger = logger;
            _resourceHelper = resourceHelper;
            _validate = validate;
            _dynamicHelper = dynamicHelper;
        }

        [Function("Post")]
        [ProducesResponseType(typeof(Models.Diversity), 201)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Created, Description = "Diversity Created", ShowSchema = true)]
        [Response(HttpStatusCode = (int)HttpStatusCode.BadRequest, Description = "Request was malformed", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Unauthorized, Description = "API key is unknown or invalid", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Forbidden, Description = "Insufficient access", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.NotFound, Description = "Resource Does Not Exist", ShowSchema = false)]
        [Response(HttpStatusCode = (int)HttpStatusCode.Conflict, Description = "Diversity Details already exists for customer", ShowSchema = false)]
        [Response(HttpStatusCode = 422, Description = "Diversity validation error(s)", ShowSchema = false)]
        [Display(Name = "Post", Description = "Ability to create a new diversity record for a given customer.")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "Customers/{customerId}/DiversityDetails")]
            HttpRequest req, string customerId)
        {
            _logger.LogTrace("Function {FunctionName} has been invoked", nameof(PostDiversityHttpTrigger));
            // Ensure the request body can be read multiple times by enabling buffering
            req.EnableBuffering();

            var correlationId = _httpRequestHelper.GetDssCorrelationId(req);

            if (!Guid.TryParse(correlationId, out var correlationGuid))
            {
                _logger.LogInformation("Unable to parse 'DssCorrelationId' to a Guid. CorrelationId: {CorrelationId}", correlationId);
                correlationGuid = Guid.NewGuid();
            }

            var touchpointId = _httpRequestHelper.GetDssTouchpointId(req);
            if (string.IsNullOrEmpty(touchpointId))
            {
                _logger.LogInformation("Unable to locate 'TouchpointId' in request header");
                return new BadRequestObjectResult("Unable to locate 'TouchpointId' in request header");
            }

            var apimUrl = _httpRequestHelper.GetDssApimUrl(req);
            if (string.IsNullOrEmpty(apimUrl))
            {
                _logger.LogInformation("Unable to locate 'apimURL' in request header. Correlation GUID: {CorrelationGuid}", correlationGuid);
                return new BadRequestObjectResult($"Unable to locate 'apimURL' in request header. Correlation GUID: {correlationGuid}");
            }

            if (!Guid.TryParse(customerId, out var customerGuid))
            {
                _logger.LogInformation("Unable to parse 'customerId' to a GUID. Customer GUID: {CustomerID}", customerId);
                return new BadRequestObjectResult($"Unable to parse 'customerId' to a GUID. Customer GUID: {customerId}");
            }
            _logger.LogTrace("Input validation has succeeded. Touchpoint ID: {TouchpointId}.", touchpointId);

            Models.Diversity diversityRequest;

            try
            {
                _logger.LogTrace("Attempting to retrieve resource from request. Correlation GUID: {CorrelationGuid}", correlationGuid);
                diversityRequest = await _httpRequestHelper.GetResourceFromRequest<Models.Diversity>(req);

                if (diversityRequest == null)
                {
                    _logger.LogInformation("{diversityRequest} object is NULL. Correlation GUID: {CorrelationGuid}\", nameof(diversityRequest), correlationGuid");
                    return new UnprocessableEntityObjectResult("Diversity Details in request body are NULL. Please supply this data.");
                }

                if (diversityRequest.ConsentToCollectEthnicity == null)
                    diversityRequest.ConsentToCollectEthnicity = false;

                if (diversityRequest.ConsentToCollectLLDDHealth == null)
                    diversityRequest.ConsentToCollectLLDDHealth = false;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Unable to parse {diversityRequest} from request body. Correlation GUID: {CorrelationGuid}. Exception: {ExceptionMessage}", nameof(diversityRequest), correlationGuid, ex.Message);
                return new UnprocessableEntityObjectResult("Unable to parse Diversity Details from request body.");
            }

            // validate the request
            _logger.LogTrace("Attempting to validate {diversityRequest} object", nameof(diversityRequest));
            var errors = _validate.ValidateResource(diversityRequest);

            if (errors != null && errors.Any())
            {
                _logger.LogInformation("Failed to validate {diversityRequest} object", nameof(diversityRequest));
                return new UnprocessableEntityObjectResult(errors);
            }
            _logger.LogTrace("Successfully validated {diversityRequest} object", nameof(diversityRequest));

            _logger.LogTrace("Checking if customer exists. Customer ID: {CustomerId}.", customerGuid);
            var doesCustomerExist = await _resourceHelper.DoesCustomerExist(customerGuid);

            if (!doesCustomerExist)
            {
                _logger.LogInformation("Customer not found. Customer ID: {CustomerId}.", customerGuid);
                return new NotFoundObjectResult($"Customer not found. Customer ID: {customerGuid}.");
            }
            _logger.LogTrace("Customer exists. Customer GUID: {CustomerGuid}.", customerGuid);

            _logger.LogTrace("Checking if customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
            var isCustomerReadOnly = _resourceHelper.IsCustomerReadOnly();

            if (isCustomerReadOnly)
            {
                var response = new ObjectResult($"Customer is read-only. Customer GUID: {customerGuid}.")
                {
                    StatusCode = (int)HttpStatusCode.Forbidden,
                };

                _logger.LogInformation("Customer is read-only. Customer GUID: {CustomerId}.", customerGuid);
                return response;
            }

            _logger.LogTrace("Checking if Diversity record already exists for customer with ID: {CustomerId}.", customerGuid);
            var doesDiversityDetailsExist = await _postDiversityService.DoesDiversityDetailsExistForCustomer(customerGuid);

            if (doesDiversityDetailsExist)
            {
                _logger.LogInformation("Diversity record already exist for customer with ID: {customerGuid}", customerGuid);
                return new ConflictObjectResult($"Diversity record already exists for customer with ID: {customerGuid}");
            }
            _logger.LogInformation("Diversity record does not exist for customer with ID: {customerGuid}", customerGuid);

            diversityRequest.SetIds(customerGuid, touchpointId);
            diversityRequest.SetDefaultValues();

            _logger.LogTrace("Attempting to create Diversity in Cosmos DB. Diversity GUID: {DiversityId}", diversityRequest.DiversityId);
            var diversity = await _postDiversityService.CreateAsync(diversityRequest);

            if (diversity == null)
            {
                _logger.LogInformation("Failed to create Diversity in Cosmos DB. Diversity GUID: {DiversityId}", diversityRequest.DiversityId);
                return new BadRequestObjectResult($"Failed to create Diversity in Cosmos DB. Diversity GUID: {diversityRequest.DiversityId}");
            }

            _logger.LogTrace("Diversity created successfully in Cosmos DB. Diversity GUID: {DiversityId}", diversity.DiversityId);


            _logger.LogTrace("Attempting to send message to Service Bus Namespace. Diversity GUID: {DiversityId}", diversity.DiversityId);
             await _postDiversityService.SendToServiceBusQueueAsync(diversityRequest, apimUrl, correlationGuid);
            _logger.LogTrace("Successfully sent message to Service Bus. Diversity GUID: {DiversityId}", diversity.DiversityId);


            _logger.LogTrace("Function {FunctionName} has finished invoking", nameof(PostDiversityHttpTrigger));
            return new JsonResult(diversity, new JsonSerializerOptions())
            {
                StatusCode = (int)HttpStatusCode.Created,
            };
        }
    }
}